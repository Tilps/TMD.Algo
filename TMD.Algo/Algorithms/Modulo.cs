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
using System.Numerics;

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
        private long[]? derangementCache;

        /// <summary>
        /// Precomputes modular derangement counts <c>!k mod basis</c> for all <c>0 &lt;= k &lt;= maxN</c> in O(maxN) time.
        /// Enables O(1) <see cref="Derangements(int)"/> lookups within the precomputed range.
        /// </summary>
        /// <param name="maxN">Maximum derangement size to precompute.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxN"/> &lt; 0.</exception>
        public void PrecomputeDerangements(int maxN)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxN);
            EnsureDerangementCache(maxN + 1);
        }

        /// <summary>
        /// Returns the number of derangements <c>!n</c> (permutations with no fixed points) modulo the basis.
        /// Expands an internal DP cache on demand in amortized O(1) time per entry.
        /// </summary>
        /// <param name="n">Number of elements, <c>n &gt;= 0</c>.</param>
        /// <returns><c>!n mod basis</c> in [0, basis - 1].</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="n"/> &lt; 0.</exception>
        public long Derangements(int n)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(n);
            if (derangementCache != null && n < derangementCache.Length)
                return derangementCache[n];

            int prevLen = derangementCache?.Length ?? 0;
            int targetLen = Math.Max(n + 1, prevLen * 2);
            EnsureDerangementCache(targetLen);
            return derangementCache![n];
        }

        private void EnsureDerangementCache(int targetLength)
        {
            int prevLen = derangementCache?.Length ?? 0;
            if (prevLen >= targetLength)
                return;

            if (derangementCache == null)
            {
                derangementCache = new long[Math.Max(targetLength, 2)];
                derangementCache[0] = 1 % basis;
                derangementCache[1] = 0;
                prevLen = 2;
            }
            else
            {
                Array.Resize(ref derangementCache, targetLength);
            }

            for (int i = prevLen; i < derangementCache.Length; i++)
            {
                long a = derangementCache[i - 1];
                long b = derangementCache[i - 2];
                long sum = a >= basis - b ? a - (basis - b) : a + b;
                derangementCache[i] = Multiply(i - 1, sum);
            }
        }

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

        /// <summary>
        /// Finds the smallest non-negative integer <c>k</c> such that <c>(k * step) mod modulus</c> lies in the
        /// target modular interval determined by <paramref name="low"/> and <paramref name="high"/> in O(log modulus) time.
        /// When <paramref name="low"/> &lt;= <paramref name="high"/>, the target interval is [<paramref name="low"/>, <paramref name="high"/>];
        /// when <paramref name="low"/> &gt; <paramref name="high"/>, it wraps around: [<paramref name="low"/>, <paramref name="modulus"/> - 1] U [0, <paramref name="high"/>].
        /// </summary>
        /// <param name="modulus">Positive modulus.</param>
        /// <param name="step">Step added per iteration (may be negative or &gt;= <paramref name="modulus"/>).</param>
        /// <param name="low">Inclusive lower endpoint in [0, <paramref name="modulus"/> - 1].</param>
        /// <param name="high">Inclusive upper endpoint in [0, <paramref name="modulus"/> - 1].</param>
        /// <returns>The smallest <c>k &gt;= 0</c>, or <c>null</c> if the target interval is never reached.</returns>
        public static long? MinStepInRange(long modulus, long step, long low, long high)
        {
            return MinStepInRange(modulus, 0L, step, low, high);
        }

        /// <summary>
        /// Finds the smallest non-negative integer <c>k</c> such that <c>(start + k * step) mod modulus</c> lies in the
        /// target modular interval determined by <paramref name="low"/> and <paramref name="high"/> in O(log modulus) time.
        /// When <paramref name="low"/> &lt;= <paramref name="high"/>, the target interval is [<paramref name="low"/>, <paramref name="high"/>];
        /// when <paramref name="low"/> &gt; <paramref name="high"/>, it wraps around: [<paramref name="low"/>, <paramref name="modulus"/> - 1] U [0, <paramref name="high"/>].
        /// </summary>
        /// <param name="modulus">Positive modulus.</param>
        /// <param name="start">Initial value before taking any steps.</param>
        /// <param name="step">Step added per iteration (may be negative or &gt;= <paramref name="modulus"/>).</param>
        /// <param name="low">Inclusive lower endpoint in [0, <paramref name="modulus"/> - 1].</param>
        /// <param name="high">Inclusive upper endpoint in [0, <paramref name="modulus"/> - 1].</param>
        /// <returns>The smallest <c>k &gt;= 0</c>, or <c>null</c> if the target interval is never reached.</returns>
        public static long? MinStepInRange(long modulus, long start, long step, long low, long high)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(modulus);
            ValidateRangeEndpoints(modulus, low, high);

            long s0 = NormalizeMod(start, modulus);
            if (IsInModularInterval(s0, low, high))
                return 0L;

            long stepMod = NormalizeMod(step, modulus);
            long shiftedLow = NormalizeMod(low - s0, modulus);
            long shiftedHigh = NormalizeMod(high - s0, modulus);
            Int128? res = MinStepPositive128(modulus, stepMod, shiftedLow, shiftedHigh);
            return res.HasValue ? (long)res.Value : null;
        }

        /// <summary>
        /// Finds the smallest non-negative <see cref="BigInteger"/> <c>k</c> such that <c>(k * step) mod modulus</c> lies in the
        /// target modular interval determined by <paramref name="low"/> and <paramref name="high"/> in O(log modulus) operations.
        /// When <paramref name="low"/> &lt;= <paramref name="high"/>, the target interval is [<paramref name="low"/>, <paramref name="high"/>];
        /// when <paramref name="low"/> &gt; <paramref name="high"/>, it wraps around: [<paramref name="low"/>, <paramref name="modulus"/> - 1] U [0, <paramref name="high"/>].
        /// </summary>
        /// <param name="modulus">Positive modulus.</param>
        /// <param name="step">Step added per iteration (may be negative or &gt;= <paramref name="modulus"/>).</param>
        /// <param name="low">Inclusive lower endpoint in [0, <paramref name="modulus"/> - 1].</param>
        /// <param name="high">Inclusive upper endpoint in [0, <paramref name="modulus"/> - 1].</param>
        /// <returns>The smallest <c>k &gt;= 0</c>, or <c>null</c> if the target interval is never reached.</returns>
        public static BigInteger? MinStepInRange(BigInteger modulus, BigInteger step, BigInteger low, BigInteger high)
        {
            return MinStepInRange(modulus, BigInteger.Zero, step, low, high);
        }

        /// <summary>
        /// Finds the smallest non-negative <see cref="BigInteger"/> <c>k</c> such that <c>(start + k * step) mod modulus</c> lies in the
        /// target modular interval determined by <paramref name="low"/> and <paramref name="high"/> in O(log modulus) operations.
        /// When <paramref name="low"/> &lt;= <paramref name="high"/>, the target interval is [<paramref name="low"/>, <paramref name="high"/>];
        /// when <paramref name="low"/> &gt; <paramref name="high"/>, it wraps around: [<paramref name="low"/>, <paramref name="modulus"/> - 1] U [0, <paramref name="high"/>].
        /// </summary>
        /// <param name="modulus">Positive modulus.</param>
        /// <param name="start">Initial value before taking any steps.</param>
        /// <param name="step">Step added per iteration (may be negative or &gt;= <paramref name="modulus"/>).</param>
        /// <param name="low">Inclusive lower endpoint in [0, <paramref name="modulus"/> - 1].</param>
        /// <param name="high">Inclusive upper endpoint in [0, <paramref name="modulus"/> - 1].</param>
        /// <returns>The smallest <c>k &gt;= 0</c>, or <c>null</c> if the target interval is never reached.</returns>
        public static BigInteger? MinStepInRange(BigInteger modulus, BigInteger start, BigInteger step, BigInteger low, BigInteger high)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(modulus);
            ValidateRangeEndpoints(modulus, low, high);

            BigInteger s0 = NormalizeMod(start, modulus);
            if (IsInModularInterval(s0, low, high))
                return BigInteger.Zero;

            BigInteger stepMod = NormalizeMod(step, modulus);
            BigInteger shiftedLow = NormalizeMod(low - s0, modulus);
            BigInteger shiftedHigh = NormalizeMod(high - s0, modulus);
            return MinStepPositiveBig(modulus, stepMod, shiftedLow, shiftedHigh);
        }

        private static void ValidateRangeEndpoints(long modulus, long low, long high)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(low);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(low, modulus);
            ArgumentOutOfRangeException.ThrowIfNegative(high);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(high, modulus);
        }

        private static void ValidateRangeEndpoints(BigInteger modulus, BigInteger low, BigInteger high)
        {
            if (low < 0 || low >= modulus)
                throw new ArgumentOutOfRangeException(nameof(low), "Endpoint must be in [0, modulus - 1].");
            if (high < 0 || high >= modulus)
                throw new ArgumentOutOfRangeException(nameof(high), "Endpoint must be in [0, modulus - 1].");
        }

        private static long NormalizeMod(long value, long modulus)
        {
            long rem = value % modulus;
            return rem < 0 ? rem + modulus : rem;
        }

        private static BigInteger NormalizeMod(BigInteger value, BigInteger modulus)
        {
            BigInteger rem = value % modulus;
            return rem.Sign < 0 ? rem + modulus : rem;
        }

        private static bool IsInModularInterval(long value, long low, long high) =>
            low <= high ? (value >= low && value <= high) : (value >= low || value <= high);

        private static bool IsInModularInterval(BigInteger value, BigInteger low, BigInteger high) =>
            low <= high ? (value >= low && value <= high) : (value >= low || value <= high);

        private static Int128? MinStepPositive128(Int128 m, Int128 s, Int128 l, Int128 r)
        {
            if (s == 0)
                return null;
            if (2 * s > m)
                return MinStepPositive128(m, m - s, m - r, m - l);
            Int128 k0 = (l + s - 1) / s;
            if (k0 * s <= r)
                return k0;
            Int128 nextS = s - (m % s);
            if (nextS == s)
                nextS = 0;
            Int128? q = MinStepPositive128(s, nextS, l % s, r % s);
            return q.HasValue ? (q.Value * m + l + s - 1) / s : null;
        }

        private static BigInteger? MinStepPositiveBig(BigInteger m, BigInteger s, BigInteger l, BigInteger r)
        {
            if (s.IsZero)
                return null;
            if ((s << 1) > m)
                return MinStepPositiveBig(m, m - s, m - r, m - l);
            BigInteger k0 = (l + s - 1) / s;
            if (k0 * s <= r)
                return k0;
            BigInteger nextS = s - (m % s);
            if (nextS == s)
                nextS = BigInteger.Zero;
            BigInteger? q = MinStepPositiveBig(s, nextS, l % s, r % s);
            return q.HasValue ? (q.Value * m + l + s - 1) / s : null;
        }

        /// <summary>
        /// Solves the system of two congruences <c>x ≡ r1 (mod m1)</c> and <c>x ≡ r2 (mod m2)</c>
        /// using the generalized Chinese Remainder Theorem in O(log max(m1, m2)) time without 64-bit overflow.
        /// </summary>
        /// <param name="r1">First remainder (may be negative or larger than <paramref name="m1"/>).</param>
        /// <param name="m1">First positive modulus.</param>
        /// <param name="r2">Second remainder (may be negative or larger than <paramref name="m2"/>).</param>
        /// <param name="m2">Second positive modulus.</param>
        /// <param name="remainder">
        /// When this method returns true, contains the unique solution in <c>[0, modulus - 1]</c>; otherwise 0.
        /// </param>
        /// <param name="modulus">
        /// When this method returns true, contains <c>lcm(m1, m2)</c>; otherwise 0.
        /// </param>
        /// <returns>
        /// True if a solution exists and <c>lcm(m1, m2)</c> fits in signed 64-bit integer range; otherwise false.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="m1"/> &lt;= 0 or <paramref name="m2"/> &lt;= 0.
        /// </exception>
        public static bool TryChineseRemainder(long r1, long m1, long r2, long m2, out long remainder, out long modulus)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(m1);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(m2);

            long a1 = NormalizeMod(r1, m1);
            long a2 = NormalizeMod(r2, m2);
            long g = MathFormulas.ExtendedGcd(m1, m2, out long x, out _);
            if ((a2 - a1) % g != 0)
            {
                remainder = 0;
                modulus = 0;
                return false;
            }

            long m2DivG = m2 / g;
            Int128 lcm128 = (Int128)(m1 / g) * m2;
            if (lcm128 > long.MaxValue)
            {
                remainder = 0;
                modulus = 0;
                return false;
            }

            Int128 diffDivG = ((Int128)a2 - a1) / g;
            long dMod = (long)(((diffDivG % m2DivG) + m2DivG) % m2DivG);
            long xMod = ((x % m2DivG) + m2DivG) % m2DivG;
            long k = (long)(((Int128)dMod * xMod) % m2DivG);
            remainder = (long)((Int128)a1 + (Int128)m1 * k);
            modulus = (long)lcm128;
            return true;
        }

        /// <summary>
        /// Solves a system of congruences <c>x ≡ r_i (mod m_i)</c> using the generalized Chinese Remainder Theorem.
        /// An empty span yields <c>remainder = 0</c> and <c>modulus = 1</c>.
        /// </summary>
        /// <param name="congruences">Sequence of <c>(Remainder, Modulus)</c> pairs with strictly positive moduli.</param>
        /// <param name="remainder">
        /// When this method returns true, contains the unique solution in <c>[0, modulus - 1]</c>; otherwise 0.
        /// </param>
        /// <param name="modulus">
        /// When this method returns true, contains the least common multiple of all moduli; otherwise 0.
        /// </param>
        /// <returns>
        /// True if all congruences are mutually consistent and the combined modulus fits in 64-bit signed range; otherwise false.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any congruence has modulus &lt;= 0.</exception>
        public static bool TryChineseRemainder(
            ReadOnlySpan<(long Remainder, long Modulus)> congruences,
            out long remainder,
            out long modulus)
        {
            for (int i = 0; i < congruences.Length; i++)
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(congruences[i].Modulus);

            long curRem = 0;
            long curMod = 1;
            for (int i = 0; i < congruences.Length; i++)
            {
                if (!TryChineseRemainder(curRem, curMod, congruences[i].Remainder, congruences[i].Modulus, out curRem, out curMod))
                {
                    remainder = 0;
                    modulus = 0;
                    return false;
                }
            }

            remainder = curRem;
            modulus = curMod;
            return true;
        }
    }
}