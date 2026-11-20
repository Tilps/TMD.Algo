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
using System.Linq;
using System.Numerics;

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Math formulas which are not specifically related to anything.
    /// </summary>
    public static class MathFormulas
    {
        /// <summary>
        /// Gets the nth harmonic number.
        /// </summary>
        /// <param name="n">
        /// Index in to the harmonic series.
        /// </param>
        /// <returns>
        /// The nth harmonic number.
        /// </returns>
        public static double HarmonicNumber(int n)
        {
            if (n > 32)
            {
                return EulerGamma + Math.Log(n) + 0.5/n - 1.0/12/n/n + 1.0/120/n/n/n/n - 1.0/252/n/n/n/n/n/n;
            }
            else
            {
                double accumulator = 0.0;
                for (int i = 0; i < n; i++)
                {
                    accumulator += 1.0/(i + 1);
                }
                return accumulator;
            }
        }

        /// <summary>
        /// Gets the difference between the harmonic number at end and the one before start.
        /// </summary>
        /// <param name="start">
        /// Start of the harmonic segment.
        /// </param>
        /// <param name="end">
        /// End of the harmonic segment.
        /// </param>
        /// <returns>
        /// The difference between the harmonic number at end and the one before start.
        /// </returns>
        public static double HarmonicSegment(int start, int end)
        {
            int before = start - 1;
            if (start > 32)
            {
                Fraction endF = new Fraction(end, 1);
                Fraction beforeF = new Fraction(before, 1);
                Fraction one = new Fraction(1, 1);
                double beforeD = before;
                double endD = end;
                double accumulator;
                if (end - before < before/10)
                    accumulator = Log1p((double)(end - before)/before);
                else
                    accumulator = Math.Log((double)end/before);
                long product = (long)before*end;
                if (product < (1L << 62))
                    accumulator += (double)(one/endF - one/beforeF)/2.0;
                else
                    accumulator += (beforeD - endD)/end/before/2.0;
                if (product < (1L << 31))
                    accumulator -= (double)(one/endF/endF - one/beforeF/beforeF)/12.0;
                else
                    accumulator -= (beforeD*beforeD - endD*endD)/end/end/before/before/12.0;
                if (product < (1 << 15))
                    accumulator += (double)(one/endF/endF/endF/endF - one/beforeF/beforeF/beforeF/beforeF)/120.0;
                else
                    accumulator += (beforeD*beforeD*beforeD*beforeD - endD*endD*endD*endD)/end/end/end/end/before/before/
                                   before/before/120.0;
                if (product < (1 << 10))
                    accumulator -=
                        (double)
                            (one/endF/endF/endF/endF/endF/endF - one/beforeF/beforeF/beforeF/beforeF/beforeF/beforeF)/
                        252.0;
                else
                    accumulator -= (beforeD*beforeD*beforeD*beforeD*beforeD*beforeD - endD*endD*endD*endD*endD*endD)/end/
                                   end/end/end/end/end/before/before/before/before/before/before/252.0;
                return accumulator;
            }
            else
            {
                return HarmonicNumber(end) - HarmonicNumber(before);
            }
        }

        /// <summary>
        /// Calculates the logarithm of a number close to 1 based on its offset.
        /// </summary>
        /// <param name="offset">
        /// Offset from 1.
        /// </param>
        /// <returns>
        /// Returns log (1+offset)
        /// </returns>
        public static double Log1p(double offset)
        {
            if (offset > 0.1 || offset < -0.1)
                return Math.Log(1 + offset);
            double accumulator = 0.0;
            double offsetPower = -offset;
            for (int i = 1; i <= 30; i++)
            {
                accumulator -= offsetPower/i;
                offsetPower *= -offset;
            }
            return accumulator;
        }

        /// <summary>
        /// The euler mascheroni constant or euler gamma.
        /// </summary>
        public const double EulerGamma = 0.57721566490153286060651209008240243104215933593992;

        /// <summary>
        /// The golden ratio constant.
        /// </summary>
        /// <remarks>
        /// Ratio of consecutive fibonacci numbers converges to this value with exponentially decaying erf.
        /// </remarks>
        public static readonly double GoldenRatio = (1.0 + Math.Sqrt(5))/2.0;

        /// <summary>
        /// Quickly determines a positive integer power of a value.
        /// Can be used to calculate the result of any repeated binary operation where the same value is used repeatedly 
        /// so long as the binary operation is associative.
        /// Effectively creates a binary tree and only evaluates equivalent subtrees once.
        /// </summary>
        /// <remarks>
        /// There is no specialization for integer or BigInteger because BigInteger.Pow/ModPow are almost certainly a better option.
        /// This is mostly useful for 'unusual' scenarios like complex numbers/matricies/strange basis vectors with a modulus to avoid
        /// values exploding to infinity.
        /// </remarks>
        /// <param name="value">
        /// Basic value.
        /// </param>
        /// <param name="power">
        /// Number of times to apply the multiplyFunc using the basic value.
        /// Must be greater than 0.
        /// </param>
        /// <param name="multiplyFunc">
        /// Function which takes 2 values and returns their multiple.
        /// </param>
        /// <typeparam name="T">
        /// Type being multiplied.
        /// </typeparam>
        /// <returns>
        /// The result of the repeated multiplication.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the power is non-positive or the multiply func is missing.
        /// </exception>
        public static T FastExponent<T>(T value, long power, Func<T, T, T> multiplyFunc)
        {
            if (power <= 0) throw new ArgumentException("Power must be positive.", nameof(power));
            ArgumentNullException.ThrowIfNull(multiplyFunc);
            long bit = 1;
            T current = value;
            T result = default!;
            bool resultSet = false;
            for (int i = 0; i < 64; i++)
            {
                if ((ulong)bit > (ulong)power) break;
                if ((bit & power) != 0)
                {
                    if (resultSet) result = multiplyFunc(result, current);
                    else
                    {
                        resultSet = true;
                        result = current;
                    }
                }
                bit <<= 1;
                current = multiplyFunc(current, current);
            }
            return result;
        }

        /// <summary>
        /// Returns an array containing all primes less than k.
        /// </summary>
        /// <param name="k">
        /// Exclusive upper bound for set of primes to return.
        /// </param>
        /// <returns>
        /// Array containing all primes less than k.
        /// </returns>
        public static int[] PrimesLessThan(int k)
        {
            bool[] composites = new bool[k];
            if (k > 0)
            {
                composites[0] = true;
            }
            if (k > 1)
            {
                composites[1] = true;
            }
            for (int i = 2; i*i < k; i++)
            {
                if (composites[i]) continue;
                for (int j = i*i; j < k; j += i)
                {
                    composites[j] = true;
                }
            }
            int size = composites.Count(a => !a);
            int[] result = new int[size];
            int index = 0;
            for (int i = 2; i < k; i++)
            {
                if (!composites[i])
                {
                    result[index] = i;
                    index++;
                }
            }
            return result;
        }

        /// <summary>
        /// Computes the probability mass function (PMF) of the Poisson binomial distribution,
        /// which represents the distribution of the number of successes in a sequence of independent,
        /// non-identically distributed Bernoulli trials with the given success probabilities.
        /// </summary>
        /// <param name="probabilities">
        /// Probabilities of success for each trial (each value must be in [0, 1]).
        /// </param>
        /// <param name="maxSuccesses">
        /// Optional maximum number of successes to compute probabilities for. If negative or greater than the number of trials,
        /// probabilities up to the total number of trials are computed.
        /// </param>
        /// <returns>
        /// An array of length min(limit, n) + 1 where element k is the probability of exactly k successes.
        /// </returns>
        public static double[] PoissonBinomialPmf(ReadOnlySpan<double> probabilities, int maxSuccesses = -1)
        {
            int n = probabilities.Length;
            int limit = (maxSuccesses < 0 || maxSuccesses > n) ? n : maxSuccesses;
            double[] dp = new double[limit + 1];
            dp[0] = 1.0;
            for (int i = 0; i < n; i++)
            {
                double p = probabilities[i];
                double oneMinusP = 1.0 - p;
                int upper = Math.Min(i + 1, limit);
                for (int j = upper; j >= 0; j--)
                {
                    dp[j] = dp[j] * oneMinusP + (j > 0 ? dp[j - 1] * p : 0.0);
                }
            }
            return dp;
        }

        /// <summary>
        /// Computes the Möbius function values for all integers from 0 to n using a sieve.
        /// </summary>
        /// <param name="n">Maximum value (inclusive) up to which to compute the Möbius function.</param>
        /// <returns>
        /// An array of length n + 1 where index i contains the Möbius value μ(i):
        /// 1 if i = 1, 0 if i is not square-free, and (-1)^k if i is square-free with k distinct prime factors.
        /// μ(0) is 0.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if n is negative.</exception>
        public static int[] MobiusSieve(int n)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(n);

            int[] mu = new int[n + 1];
            if (n == 0)
                return mu;

            mu[1] = 1;
            bool[] isComposite = new bool[n + 1];
            List<int> primes = [];

            for (int i = 2; i <= n; i++)
            {
                if (!isComposite[i])
                {
                    primes.Add(i);
                    mu[i] = -1;
                }

                for (int j = 0; j < primes.Count && (long)i * primes[j] <= n; j++)
                {
                    int p = primes[j];
                    isComposite[i * p] = true;
                    if (i % p == 0)
                    {
                        mu[i * p] = 0;
                        break;
                    }
                    mu[i * p] = -mu[i];
                }
            }

            return mu;
        }

        /// <summary>
        /// Computes Euler's totient function φ(i) for all integers from 0 to n using a sieve.
        /// </summary>
        /// <param name="n">Maximum value (inclusive) up to which to compute Euler's totient function.</param>
        /// <returns>
        /// An array of length n + 1 where index i contains φ(i), the count of integers 1 ≤ k ≤ i coprime to i.
        /// φ(0) is 0 and φ(1) is 1.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if n is negative.</exception>
        public static int[] EulerTotientSieve(int n)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(n);

            int[] phi = new int[n + 1];
            if (n == 0)
                return phi;

            phi[1] = 1;
            bool[] isComposite = new bool[n + 1];
            List<int> primes = [];

            for (int i = 2; i <= n; i++)
            {
                if (!isComposite[i])
                {
                    primes.Add(i);
                    phi[i] = i - 1;
                }

                for (int j = 0; j < primes.Count && (long)i * primes[j] <= n; j++)
                {
                    int p = primes[j];
                    isComposite[i * p] = true;
                    if (i % p == 0)
                    {
                        phi[i * p] = phi[i] * p;
                        break;
                    }
                    phi[i * p] = phi[i] * (p - 1);
                }
            }

            return phi;
        }

        /// <summary>
        /// Computes the greatest common divisor of two 64-bit integers using the Euclidean algorithm.
        /// </summary>
        /// <param name="a">First integer.</param>
        /// <param name="b">Second integer.</param>
        /// <returns>The greatest common divisor, which is always non-negative.</returns>
        public static long Gcd(long a, long b)
        {
            while (b != 0)
            {
                long t = a % b;
                a = b;
                b = t;
            }
            return Math.Abs(a);
        }

        /// <summary>
        /// Computes the greatest common divisor of two 32-bit integers using the Euclidean algorithm.
        /// </summary>
        /// <param name="a">First integer.</param>
        /// <param name="b">Second integer.</param>
        /// <returns>The greatest common divisor, which is always non-negative.</returns>
        public static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                int t = a % b;
                a = b;
                b = t;
            }
            return Math.Abs(a);
        }

        /// <summary>
        /// Computes the least common multiple of two 64-bit integers.
        /// </summary>
        /// <param name="a">First integer.</param>
        /// <param name="b">Second integer.</param>
        /// <returns>The least common multiple, which is always non-negative.</returns>
        public static long Lcm(long a, long b)
        {
            if (a == 0 || b == 0) return 0;
            return Math.Abs((a / Gcd(a, b)) * b);
        }

        /// <summary>
        /// Computes the non-negative greatest common divisor of <paramref name="a"/> and <paramref name="b"/>
        /// together with Bézout coefficients satisfying a * x + b * y = gcd(a, b).
        /// </summary>
        /// <param name="a">First integer.</param>
        /// <param name="b">Second integer.</param>
        /// <param name="x">Coefficient multiplying <paramref name="a"/>.</param>
        /// <param name="y">Coefficient multiplying <paramref name="b"/>.</param>
        /// <returns>The non-negative greatest common divisor of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static long ExtendedGcd(long a, long b, out long x, out long y)
        {
            long oldR = a, r = b;
            long oldS = 1, s = 0;
            long oldT = 0, t = 1;
            while (r != 0)
            {
                long q = oldR / r;
                (oldR, r) = (r, oldR - q * r);
                (oldS, s) = (s, oldS - q * s);
                (oldT, t) = (t, oldT - q * t);
            }
            if (oldR < 0)
            {
                oldR = -oldR;
                oldS = -oldS;
                oldT = -oldT;
            }
            x = oldS;
            y = oldT;
            return oldR;
        }

        /// <summary>
        /// Computes integer division rounded down towards negative infinity (floor division).
        /// </summary>
        /// <param name="num">The dividend numerator.</param>
        /// <param name="den">The divisor denominator.</param>
        /// <returns>The quotient rounded towards negative infinity.</returns>
        /// <exception cref="DivideByZeroException">Thrown if <paramref name="den"/> is zero.</exception>
        public static long DivFloor(long num, long den)
        {
            if (den == 0) throw new DivideByZeroException("Denominator cannot be zero.");
            if (den < 0) { num = -num; den = -den; }
            return num >= 0 ? num / den : (num - den + 1) / den;
        }

        /// <summary>
        /// Computes integer division rounded up towards positive infinity (ceil division).
        /// </summary>
        /// <param name="num">The dividend numerator.</param>
        /// <param name="den">The divisor denominator.</param>
        /// <returns>The quotient rounded towards positive infinity.</returns>
        /// <exception cref="DivideByZeroException">Thrown if <paramref name="den"/> is zero.</exception>
        public static long DivCeil(long num, long den)
        {
            if (den == 0) throw new DivideByZeroException("Denominator cannot be zero.");
            if (den < 0) { num = -num; den = -den; }
            return num >= 0 ? (num + den - 1) / den : num / den;
        }

        /// <summary>
        /// Computes integer division rounded down towards negative infinity (floor division).
        /// </summary>
        /// <param name="num">The dividend numerator.</param>
        /// <param name="den">The divisor denominator.</param>
        /// <returns>The quotient rounded towards negative infinity.</returns>
        /// <exception cref="DivideByZeroException">Thrown if <paramref name="den"/> is zero.</exception>
        public static int DivFloor(int num, int den)
        {
            if (den == 0) throw new DivideByZeroException("Denominator cannot be zero.");
            if (den < 0) { num = -num; den = -den; }
            return num >= 0 ? num / den : (num - den + 1) / den;
        }

        /// <summary>
        /// Determines whether <paramref name="value"/> is a prime number using deterministic 64-bit Miller-Rabin.
        /// </summary>
        /// <param name="value">Integer to test for primality.</param>
        /// <returns>True if <paramref name="value"/> is prime; otherwise, false.</returns>
        public static bool IsPrime(long value)
        {
            if (value < 2)
                return false;

            ReadOnlySpan<long> smallPrimes = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37];
            foreach (long p in smallPrimes)
            {
                if (value == p)
                    return true;
                if (value % p == 0)
                    return false;
            }
            if (value < 41L * 41L)
                return true;

            long d = value - 1;
            int s = BitOperations.TrailingZeroCount((ulong)d);
            d >>= s;

            ReadOnlySpan<long> bases = [2L, 325L, 9375L, 28178L, 450775L, 9780504L, 1795265022L];
            foreach (long a in bases)
            {
                if (a % value == 0)
                    continue;
                long x = Modulo.Pow(a, d, value);
                if (x == 1 || x == value - 1)
                    continue;
                bool composite = true;
                for (int r = 1; r < s; r++)
                {
                    x = Modulo.Multiply(x, x, value);
                    if (x == value - 1)
                    {
                        composite = false;
                        break;
                    }
                }
                if (composite)
                    return false;
            }
            return true;
        }

        private static ulong GcdUInt64(ulong a, ulong b)
        {
            while (b != 0)
            {
                ulong t = a % b;
                a = b;
                b = t;
            }
            return a;
        }

        /// <summary>
        /// Finds a non-trivial divisor of an odd composite integer <paramref name="n"/> using Brent's cycle-detection
        /// variant of Pollard's rho algorithm in expected O(n^(1/4)) time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The iteration generates a pseudorandom walk <c>z_{k+1} = (z_k^2 + c) mod n</c>. Modulo any unknown prime
        /// factor <c>p | n</c>, the sequence enters a cycle after roughly <c>O(sqrt(p)) &lt;= O(n^(1/4))</c> steps by
        /// the birthday paradox, so two indices separated by the cycle length have <c>|x - y|</c> divisible by <c>p</c>.
        /// </para>
        /// <para>
        /// Brent's variant improves on Floyd's tortoise-and-hare by teleporting the tortoise <c>x</c> to the hare
        /// <c>y</c> at powers of two (<c>r = 1, 2, 4, ...</c>) and batching modular differences in a product <c>q</c>
        /// before taking <c>Gcd(q, n)</c> once every <c>m = 128</c> steps. If batching overshoots (<c>gcd == n</c>),
        /// it rewinds to checkpoint <c>ys</c> and steps singly until a factor emerges; if all factors collide at once
        /// (<c>gcd == n</c>), it restarts with the next constant <c>c</c>.
        /// </para>
        /// </remarks>
        /// <param name="n">Composite positive integer with no small prime factors.</param>
        /// <returns>A non-trivial divisor <c>d</c> satisfying <c>1 &lt; d &lt; n</c>.</returns>
        private static long PollardRhoBrent(long n)
        {
            for (long c = 1; ; c++)
            {
                long cMod = c % n;

                long Step(long val)
                {
                    long sq = Modulo.Multiply(val, val, n);
                    return sq >= n - cMod ? sq - (n - cMod) : sq + cMod;
                }

                long y = (c % (n - 1)) + 1;
                long x = y;
                long q = 1;
                const long m = 128;
                long g = 1;
                long r = 1;
                long ys = 0;

                while (g == 1)
                {
                    // Teleport tortoise x to hare y and expand the search horizon r by a factor of 2.
                    x = y;
                    for (long i = 0; i < r; i++)
                        y = Step(y);

                    long k = 0;
                    while (k < r && g == 1)
                    {
                        // Save checkpoint ys before processing a batch of m steps for rollback if gcd overshoots to n.
                        ys = y;
                        long lim = Math.Min(m, r - k);
                        for (long i = 0; i < lim; i++)
                        {
                            y = Step(y);
                            long diff = x > y ? x - y : y - x;
                            q = Modulo.Multiply(q, diff, n);
                        }
                        g = Gcd(q, n);
                        k += m;
                    }
                    r <<= 1;
                }

                // Batch product captured n (or diff == 0); rewind to ys and inspect single steps.
                if (g == n)
                {
                    do
                    {
                        ys = Step(ys);
                        long diff = x > ys ? x - ys : ys - x;
                        g = Gcd(diff, n);
                    }
                    while (g == 1);
                }

                // If g is a proper divisor, return it; otherwise try a different polynomial offset c.
                if (g != n)
                    return g;
            }
        }

        private static void FactorRecursive(long n, List<long> factors)
        {
            if (n <= 1)
                return;
            if (IsPrime(n))
            {
                factors.Add(n);
                return;
            }
            long d = PollardRhoBrent(n);
            FactorRecursive(d, factors);
            FactorRecursive(n / d, factors);
        }

        /// <summary>
        /// Computes the prime factorization of a positive 64-bit integer using small-prime trial division
        /// followed by deterministic Miller-Rabin and Brent's Pollard's rho algorithm.
        /// </summary>
        /// <remarks>
        /// Factors are extracted in three stages: (1) trial division by primes up to 97, (2) deterministic
        /// 64-bit Miller-Rabin primality testing, and (3) recursive splitting via Brent's variant of Pollard's rho.
        /// </remarks>
        /// <param name="value">Positive integer to factorize.</param>
        /// <returns>List of (Prime, Exponent) pairs ordered by ascending prime.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> &lt;= 0.</exception>
        public static List<(long Prime, int Exponent)> PrimeFactorize(long value)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            List<(long Prime, int Exponent)> result = [];
            if (value == 1)
                return result;

            ReadOnlySpan<int> smallPrimes =
            [
                2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37,
                41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97
            ];

            long rem = value;
            foreach (int p in smallPrimes)
            {
                if ((long)p * p > rem)
                    break;
                if (rem % p == 0)
                {
                    int exp = 0;
                    do
                    {
                        rem /= p;
                        exp++;
                    }
                    while (rem % p == 0);
                    result.Add((p, exp));
                }
            }

            if (rem > 1)
            {
                List<long> largeFactors = [];
                FactorRecursive(rem, largeFactors);
                largeFactors.Sort();
                for (int i = 0; i < largeFactors.Count;)
                {
                    long p = largeFactors[i];
                    int j = i + 1;
                    while (j < largeFactors.Count && largeFactors[j] == p)
                        j++;
                    result.Add((p, j - i));
                    i = j;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all positive divisors of <paramref name="value"/> in ascending order.
        /// </summary>
        /// <param name="value">Positive integer whose divisors to enumerate.</param>
        /// <returns>Sorted array of positive divisors of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> &lt;= 0.</exception>
        public static long[] GetDivisors(long value)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            List<(long Prime, int Exponent)> factors = PrimeFactorize(value);
            List<long> divisors = [1L];
            foreach (var (prime, exp) in factors)
            {
                int curCount = divisors.Count;
                long mult = prime;
                for (int e = 1; e <= exp; e++)
                {
                    for (int i = 0; i < curCount; i++)
                        divisors.Add(divisors[i] * mult);
                    if (e < exp)
                        mult *= prime;
                }
            }
            long[] sorted = [.. divisors];
            Array.Sort(sorted);
            return sorted;
        }

        /// <summary>
        /// Computes the least common multiple of <paramref name="a"/> and <paramref name="b"/> without overflow
        /// if the non-negative result does not exceed <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="a">First integer.</param>
        /// <param name="b">Second integer.</param>
        /// <param name="maxValue">Maximum allowed non-negative result.</param>
        /// <param name="lcm">When this method returns true, contains the non-negative LCM; otherwise 0.</param>
        /// <returns>True if the LCM is at most <paramref name="maxValue"/>; otherwise false.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxValue"/> &lt; 0.</exception>
        public static bool TryLcm(long a, long b, long maxValue, out long lcm)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxValue);
            if (a == 0 || b == 0)
            {
                lcm = 0;
                return true;
            }
            ulong ua = (ulong)(a < 0 ? -(Int128)a : (Int128)a);
            ulong ub = (ulong)(b < 0 ? -(Int128)b : (Int128)b);
            ulong g = GcdUInt64(ua, ub);
            UInt128 prod = (UInt128)(ua / g) * ub;
            if (prod > (UInt128)maxValue)
            {
                lcm = 0;
                return false;
            }
            lcm = (long)prod;
            return true;
        }

        /// <summary>
        /// Computes the exact integer floor square root of a non-negative 64-bit integer.
        /// </summary>
        /// <param name="value">Non-negative integer.</param>
        /// <returns>Largest integer y such that y * y &lt;= <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> &lt; 0.</exception>
        public static long IntegerSqrt(long value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            return (long)IntegerSqrt((UInt128)(ulong)value);
        }

        /// <summary>
        /// Computes the exact integer floor square root of an unsigned 128-bit integer.
        /// </summary>
        /// <param name="value">Unsigned 128-bit integer.</param>
        /// <returns>Largest 64-bit unsigned integer y such that y * y &lt;= <paramref name="value"/>.</returns>
        [CLSCompliant(false)]
        public static ulong IntegerSqrt(UInt128 value)
        {
            if (value == 0)
                return 0;
            int shift = (128 - (int)UInt128.LeadingZeroCount(value) + 1) >> 1;
            UInt128 x = (UInt128)1 << shift;
            while (true)
            {
                UInt128 y = (x + value / x) >> 1;
                if (y >= x)
                    return (ulong)x;
                x = y;
            }
        }

        /// <summary>
        /// Computes the exact integer floor square root of a non-negative arbitrary-precision integer.
        /// </summary>
        /// <param name="value">Non-negative <see cref="BigInteger"/>.</param>
        /// <returns>Largest integer y such that y * y &lt;= <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> &lt; 0.</exception>
        public static BigInteger IntegerSqrt(BigInteger value)
        {
            if (value.Sign < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");
            if (value.IsZero)
                return BigInteger.Zero;
            int shift = checked((int)((value.GetBitLength() + 1) >> 1));
            BigInteger x = BigInteger.One << shift;
            while (true)
            {
                BigInteger y = (x + value / x) >> 1;
                if (y >= x)
                    return x;
                x = y;
            }
        }

        /// <summary>
        /// Computes integer division rounded up towards positive infinity (ceil division).
        /// </summary>
        /// <param name="num">The dividend numerator.</param>
        /// <param name="den">The divisor denominator.</param>
        /// <returns>The quotient rounded towards positive infinity.</returns>
        /// <exception cref="DivideByZeroException">Thrown if <paramref name="den"/> is zero.</exception>
        public static int DivCeil(int num, int den)
        {
            if (den == 0) throw new DivideByZeroException("Denominator cannot be zero.");
            if (den < 0) { num = -num; den = -den; }
            return num >= 0 ? (num + den - 1) / den : num / den;
        }
    }
}