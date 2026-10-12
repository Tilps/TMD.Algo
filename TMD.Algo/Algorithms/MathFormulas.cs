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
            if (multiplyFunc == null) throw new ArgumentNullException(nameof(multiplyFunc));
            long bit = 1;
            T current = value;
            T result = default(T);
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
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n), "Upper bound must be non-negative.");

            int[] mu = new int[n + 1];
            if (n == 0)
                return mu;

            mu[1] = 1;
            bool[] isComposite = new bool[n + 1];
            List<int> primes = new List<int>();

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
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n), "Upper bound must be non-negative.");

            int[] phi = new int[n + 1];
            if (n == 0)
                return phi;

            phi[1] = 1;
            bool[] isComposite = new bool[n + 1];
            List<int> primes = new List<int>();

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
    }
}