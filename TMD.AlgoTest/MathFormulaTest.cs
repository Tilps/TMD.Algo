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
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class MathFormulaTest
    {
        [Test]
        public void Basic()
        {
            double res = MathFormulas.Log1p(0.00001);
            Assert.AreEqual(0.00001, Math.Exp(res) - 1.0, 0.000000000001);

            Assert.AreEqual(1.0/1000000, MathFormulas.HarmonicSegment(1000000, 1000000), 0.000000001);

            // Test small Harmonic numbers
            Assert.AreEqual(1.0, MathFormulas.HarmonicNumber(1), 0.0000001);
            Assert.AreEqual(1.5, MathFormulas.HarmonicNumber(2), 0.0000001);
            Assert.AreEqual(1.0 + 0.5 + 1.0/3.0 + 0.25, MathFormulas.HarmonicNumber(4), 0.0000001);

            // Test Harmonic number for n > 32
            double expected50 = 0.0;
            for (int i = 1; i <= 50; i++) expected50 += 1.0 / i;
            Assert.AreEqual(expected50, MathFormulas.HarmonicNumber(50), 0.0001);
        }

        [Test]
        public void PoissonBinomialTest()
        {
            // Fair coins: Binomial(3, 0.5)
            double[] pmfFair = MathFormulas.PoissonBinomialPmf(new double[] { 0.5, 0.5, 0.5 });
            Assert.AreEqual(4, pmfFair.Length);
            Assert.AreEqual(0.125, pmfFair[0], 1e-9);
            Assert.AreEqual(0.375, pmfFair[1], 1e-9);
            Assert.AreEqual(0.375, pmfFair[2], 1e-9);
            Assert.AreEqual(0.125, pmfFair[3], 1e-9);

            // Deterministic trials
            double[] pmfDet = MathFormulas.PoissonBinomialPmf(new double[] { 0.0, 1.0 });
            Assert.AreEqual(0.0, pmfDet[0], 1e-9);
            Assert.AreEqual(1.0, pmfDet[1], 1e-9);
            Assert.AreEqual(0.0, pmfDet[2], 1e-9);

            // maxSuccesses limit
            double[] pmfLimited = MathFormulas.PoissonBinomialPmf(new double[] { 0.5, 0.5, 0.5 }, maxSuccesses: 1);
            Assert.AreEqual(2, pmfLimited.Length);
            Assert.AreEqual(0.125, pmfLimited[0], 1e-9);
            Assert.AreEqual(0.375, pmfLimited[1], 1e-9);
        }

        [Test]
        public void MobiusSieveTest()
        {
            int[] expectedSmall = { 0, 1, -1, -1, 0, -1, 1, -1, 0, 0, 1, -1, 0, -1, 1, 1 };
            int[] mu = MathFormulas.MobiusSieve(15);
            Assert.AreEqual(16, mu.Length);
            for (int i = 0; i <= 15; i++)
            {
                Assert.AreEqual(expectedSmall[i], mu[i], $"Mismatch at index {i}");
            }

            // Test divisor sum property: sum_{d | n} mu(d) == 0 for all n > 1
            int n = 500;
            int[] muLarge = MathFormulas.MobiusSieve(n);
            for (int i = 2; i <= n; i++)
            {
                int sum = 0;
                for (int d = 1; d <= i; d++)
                {
                    if (i % d == 0) sum += muLarge[d];
                }
                Assert.AreEqual(0, sum, $"Divisor sum failed for n = {i}");
            }

            // Boundary conditions
            Assert.AreEqual(1, MathFormulas.MobiusSieve(0).Length);
            Assert.AreEqual(0, MathFormulas.MobiusSieve(0)[0]);
            Assert.Throws<ArgumentOutOfRangeException>(() => MathFormulas.MobiusSieve(-1));
        }

        [Test]
        public void EulerTotientSieveTest()
        {
            int[] expectedSmall = { 0, 1, 1, 2, 2, 4, 2, 6, 4, 6, 4, 10, 4, 12, 6, 8 };
            int[] phi = MathFormulas.EulerTotientSieve(15);
            Assert.AreEqual(16, phi.Length);
            for (int i = 0; i <= 15; i++)
            {
                Assert.AreEqual(expectedSmall[i], phi[i], $"Mismatch at index {i}");
            }

            // Test Gauss's theorem: sum_{d | n} phi(d) == n for all n >= 1
            int n = 500;
            int[] phiLarge = MathFormulas.EulerTotientSieve(n);
            for (int i = 1; i <= n; i++)
            {
                int sum = 0;
                for (int d = 1; d <= i; d++)
                {
                    if (i % d == 0) sum += phiLarge[d];
                }
                Assert.AreEqual(i, sum, $"Gauss theorem failed for n = {i}");
            }

            // Boundary conditions
            Assert.AreEqual(1, MathFormulas.EulerTotientSieve(0).Length);
            Assert.AreEqual(0, MathFormulas.EulerTotientSieve(0)[0]);
            Assert.Throws<ArgumentOutOfRangeException>(() => MathFormulas.EulerTotientSieve(-1));
        }

        [Test]
        public void GcdAndLcmTest()
        {
            Assert.AreEqual(6, MathFormulas.Gcd(12, 18));
            Assert.AreEqual(6L, MathFormulas.Gcd(12L, 18L));
            Assert.AreEqual(7, MathFormulas.Gcd(7, 0));
            Assert.AreEqual(7, MathFormulas.Gcd(0, 7));
            Assert.AreEqual(5, MathFormulas.Gcd(-10, 15));
            Assert.AreEqual(5, MathFormulas.Gcd(15, -10));
            Assert.AreEqual(1, MathFormulas.Gcd(13, 17));

            Assert.AreEqual(36L, MathFormulas.Lcm(12L, 18L));
            Assert.AreEqual(0L, MathFormulas.Lcm(0L, 5L));
            Assert.AreEqual(0L, MathFormulas.Lcm(5L, 0L));
            Assert.AreEqual(221L, MathFormulas.Lcm(13L, 17L));

            Assert.AreEqual(6L, Fraction.Gcd(12L, 18L));
        }

        [Test]
        public void DivFloorAndCeilTest()
        {
            // Positive denominator
            Assert.AreEqual(2, MathFormulas.DivFloor(7, 3));
            Assert.AreEqual(3, MathFormulas.DivCeil(7, 3));
            Assert.AreEqual(2, MathFormulas.DivFloor(6, 3));
            Assert.AreEqual(2, MathFormulas.DivCeil(6, 3));

            // Negative numerator, positive denominator
            Assert.AreEqual(-3, MathFormulas.DivFloor(-7, 3));
            Assert.AreEqual(-2, MathFormulas.DivCeil(-7, 3));
            Assert.AreEqual(-2, MathFormulas.DivFloor(-6, 3));
            Assert.AreEqual(-2, MathFormulas.DivCeil(-6, 3));

            // Positive numerator, negative denominator
            Assert.AreEqual(-3, MathFormulas.DivFloor(7, -3));
            Assert.AreEqual(-2, MathFormulas.DivCeil(7, -3));

            // Both negative
            Assert.AreEqual(2, MathFormulas.DivFloor(-7, -3));
            Assert.AreEqual(3, MathFormulas.DivCeil(-7, -3));

            // Long overloads
            Assert.AreEqual(2L, MathFormulas.DivFloor(7L, 3L));
            Assert.AreEqual(3L, MathFormulas.DivCeil(7L, 3L));
            Assert.AreEqual(-3L, MathFormulas.DivFloor(-7L, 3L));
            Assert.AreEqual(-2L, MathFormulas.DivCeil(-7L, 3L));

            // Zero denominator throws
            Assert.Throws<DivideByZeroException>(() => MathFormulas.DivFloor(1, 0));
            Assert.Throws<DivideByZeroException>(() => MathFormulas.DivCeil(1, 0));
            Assert.Throws<DivideByZeroException>(() => MathFormulas.DivFloor(1L, 0L));
            Assert.Throws<DivideByZeroException>(() => MathFormulas.DivCeil(1L, 0L));
        }

        [Test]
        public void ExtendedGcdTest()
        {
            foreach ((long a, long b) in new[] { (12L, 18L), (-10L, 15L), (0L, -7L), (0L, 0L), (101L, 37L), (-42L, -30L) })
            {
                long g = MathFormulas.ExtendedGcd(a, b, out long x, out long y);
                Assert.AreEqual(MathFormulas.Gcd(a, b), g);
                Assert.AreEqual(g, a * x + b * y);
            }
        }

        [Test]
        public void IsPrimeTest()
        {
            // Non-primes including negatives, 0, 1, composites, and Carmichael numbers
            foreach (long v in new[]
            {
                -7L, 0L, 1L, 4L, 9L, 25L, 561L, 1105L, 1729L, 2465L, 2821L,
                341550071728321L, 9_223_372_036_854_775_807L
            })
            {
                Assert.IsFalse(MathFormulas.IsPrime(v), $"Expected {v} to be composite.");
            }
            Assert.IsFalse(MathFormulas.IsPrime(999_999_937L * 999_999_929L));

            foreach (long p in new[]
            {
                2L, 3L, 5L, 37L, 41L, 1681L - 2L, 1000003L, 1_000_000_007L,
                1_000_000_000_039L, 2_147_483_647L, 9_223_372_036_854_775_783L
            })
            {
                bool expectedPrime = p != 1679L; // 1679 = 23 * 73
                Assert.AreEqual(expectedPrime, MathFormulas.IsPrime(p), $"Primality check failed for {p}");
            }
        }

        [Test]
        public void PrimeFactorizeAndDivisorsTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MathFormulas.PrimeFactorize(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => MathFormulas.PrimeFactorize(-10));
            Assert.Throws<ArgumentOutOfRangeException>(() => MathFormulas.GetDivisors(0));

            Assert.IsEmpty(MathFormulas.PrimeFactorize(1));
            CollectionAssert.AreEqual(new[] { 1L }, MathFormulas.GetDivisors(1));

            var f360 = MathFormulas.PrimeFactorize(360);
            CollectionAssert.AreEqual(new[] { (2L, 3), (3L, 2), (5L, 1) }, f360);
            CollectionAssert.AreEqual(
                new[] { 1L, 2L, 3L, 4L, 5L, 6L, 8L, 9L, 10L, 12L, 15L, 18L, 20L, 24L, 30L, 36L, 40L, 45L, 60L, 72L, 90L, 120L, 180L, 360L },
                MathFormulas.GetDivisors(360));

            long semiprime = 100_000_007L * 100_000_037L;
            var fSemi = MathFormulas.PrimeFactorize(semiprime);
            CollectionAssert.AreEqual(new[] { (100_000_007L, 1), (100_000_037L, 1) }, fSemi);
            CollectionAssert.AreEqual(new[] { 1L, 100_000_007L, 100_000_037L, semiprime }, MathFormulas.GetDivisors(semiprime));

            // long.MaxValue = 7^2 * 73 * 127 * 337 * 92737 * 649657
            var fMax = MathFormulas.PrimeFactorize(long.MaxValue);
            long reconstructed = 1;
            foreach (var (prime, exp) in fMax)
            {
                Assert.IsTrue(MathFormulas.IsPrime(prime));
                for (int i = 0; i < exp; i++) reconstructed *= prime;
            }
            Assert.AreEqual(long.MaxValue, reconstructed);
        }

        [Test]
        public void TryLcmTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MathFormulas.TryLcm(2, 3, -1, out _));

            Assert.IsTrue(MathFormulas.TryLcm(0, 123, 100, out long zeroLcm));
            Assert.AreEqual(0L, zeroLcm);

            Assert.IsTrue(MathFormulas.TryLcm(-12, 18, 36, out long lcm36));
            Assert.AreEqual(36L, lcm36);

            Assert.IsFalse(MathFormulas.TryLcm(12, 18, 35, out long over));
            Assert.AreEqual(0L, over);

            // Large values that overflow long if multiplied naively
            Assert.IsFalse(MathFormulas.TryLcm(10_000_000_000_001L, 10_000_000_000_003L, 1_000_000_000_000_000_0L, out _));
            Assert.IsTrue(MathFormulas.TryLcm(6_000_000_000L, 4_000_000_000L, 20_000_000_000L, out long lcmBig));
            Assert.AreEqual(12_000_000_000L, lcmBig);
        }

        [Test]
        public void IntegerSqrtTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MathFormulas.IntegerSqrt(-1L));
            Assert.Throws<ArgumentOutOfRangeException>(() => MathFormulas.IntegerSqrt(new BigInteger(-1)));

            Assert.AreEqual(0L, MathFormulas.IntegerSqrt(0L));
            Assert.AreEqual(0UL, MathFormulas.IntegerSqrt((UInt128)0));
            Assert.AreEqual(BigInteger.Zero, MathFormulas.IntegerSqrt(BigInteger.Zero));

            for (long k = 1; k <= 20; k++)
            {
                long sq = k * k;
                Assert.AreEqual(k - 1, MathFormulas.IntegerSqrt(sq - 1));
                Assert.AreEqual(k, MathFormulas.IntegerSqrt(sq));
                Assert.AreEqual(k, MathFormulas.IntegerSqrt(sq + 1));
            }

            long rootMax = MathFormulas.IntegerSqrt(long.MaxValue);
            Assert.IsTrue((UInt128)rootMax * (UInt128)rootMax <= (UInt128)long.MaxValue);
            Assert.IsTrue((UInt128)(rootMax + 1) * (UInt128)(rootMax + 1) > (UInt128)long.MaxValue);

            ulong u128Root = MathFormulas.IntegerSqrt(UInt128.MaxValue);
            Assert.AreEqual(ulong.MaxValue, u128Root);

            UInt128 exact128 = (UInt128)123_456_789_012_345_678UL * 123_456_789_012_345_678UL;
            Assert.AreEqual(123_456_789_012_345_677UL, MathFormulas.IntegerSqrt(exact128 - 1));
            Assert.AreEqual(123_456_789_012_345_678UL, MathFormulas.IntegerSqrt(exact128));
            Assert.AreEqual(123_456_789_012_345_678UL, MathFormulas.IntegerSqrt(exact128 + 1));

            BigInteger bigBase = BigInteger.One << 100;
            BigInteger bigSq = bigBase * bigBase;
            Assert.AreEqual(bigBase - 1, MathFormulas.IntegerSqrt(bigSq - 1));
            Assert.AreEqual(bigBase, MathFormulas.IntegerSqrt(bigSq));
            Assert.AreEqual(bigBase, MathFormulas.IntegerSqrt(bigSq + 1));
        }
    }
}