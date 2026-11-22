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
using NUnit.Framework;
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class ModuloTest
    {
        [Test]
        public void Basic()
        {
            Modulo modulator = new Modulo(3);
            Assert.AreEqual(2, modulator.Add(-2, 1));
            Assert.AreEqual(1, modulator.Mod(121));
            Assert.AreEqual(0, modulator.Subtract(120, 111));
            Assert.AreEqual(2, modulator.Subtract(120, 127));
            for (int i = 1; i < 3; i++)
                Assert.AreEqual(1, modulator.Multiply(i, modulator.Invert(i)));

            Assert.AreEqual(BigIntegerPermutations(8, 2, 3), modulator.Permutations(8, 2));
            Assert.AreEqual(BigIntegerChoose(6, 3, 3), modulator.Choose(6, 3));
        }

        [Test]
        public void StaticInvertTest()
        {
            Assert.AreEqual(4L, Modulo.Invert(3, 11));
            Assert.AreEqual(4L, Modulo.Invert(-8, 11));
            Assert.AreEqual(7L, Modulo.Invert(7, 12));
            Assert.AreEqual(0L, Modulo.Invert(5, 1));

            foreach ((long x, long p) in new[]
            {
                (123_456_789L, 1_000_000_007L),
                (900_000_000_000L, 1_000_000_000_039L)
            })
            {
                Assert.AreEqual(1L, BigIntegerMultiply(x, Modulo.Invert(x, p), p));
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => Modulo.Invert(1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Modulo.Invert(1, -5));
            Assert.Throws<ArgumentException>(() => Modulo.Invert(6, 9));
            Assert.Throws<ArgumentException>(() => Modulo.Invert(0, 7));
        }

        [Test]
        public void DistTest()
        {
            Modulo mod10 = new Modulo(10);
            Assert.AreEqual(0, mod10.Dist(3, 3));
            Assert.AreEqual(0, mod10.Distance(3, 3));
            Assert.AreEqual(1, mod10.Dist(0, 9));
            Assert.AreEqual(1, mod10.Dist(9, 0));
            Assert.AreEqual(2, mod10.Dist(1, 9));
            Assert.AreEqual(2, mod10.Dist(9, 1));
            Assert.AreEqual(5, mod10.Dist(0, 5));
            Assert.AreEqual(5, mod10.Dist(5, 0));
            Assert.AreEqual(4, mod10.Dist(2, 6));

            // Negative coordinates and large offsets
            Assert.AreEqual(1, mod10.Dist(-1, 0));
            Assert.AreEqual(2, mod10.Dist(-1, 1));
            Assert.AreEqual(2, mod10.Dist(19, 1));

            // Independent confirmation against explicit min of directional subtractions
            Modulo mod24 = new Modulo(24);
            for (int i = -30; i <= 30; i++)
            {
                for (int j = -30; j <= 30; j++)
                {
                    long expected = Math.Min(mod24.Subtract(i, j), mod24.Subtract(j, i));
                    Assert.AreEqual(expected, mod24.Dist(i, j));
                    Assert.AreEqual(expected, mod24.Distance(i, j));
                }
            }
        }

        [Test]
        public void LargeNumbers()
        {
            long basis = 1_000_000_007L;
            Modulo modulator = new Modulo(basis);

            long a = 1_000_000_000L;
            long b = 1_000_000_000L;
            // Independent expected value via BigInteger:
            long expected = BigIntegerMultiply(a, b, basis);
            Assert.AreEqual(expected, modulator.Multiply(a, b));

            long bigMod = 1_000_000_000_039L; // prime > 10^12
            Modulo bigModulator = new Modulo(bigMod);
            long x = 900_000_000_000L;
            long y = 800_000_000_000L;
            // x * y = 7.2 * 10^23, which exceeds long.MaxValue (9.22 * 10^18):
            long expectedBig = BigIntegerMultiply(x, y, bigMod);
            Assert.AreEqual(expectedBig, bigModulator.Multiply(x, y));
        }

        [Test]
        public void IndependentBigIntegerVerification()
        {
            long[] bases = { 3L, 7L, 101L, 1_000_000_007L, 1_000_000_000_039L };
            Random rnd = new Random(42);

            foreach (long basis in bases)
            {
                Modulo modulator = new Modulo(basis);

                for (int i = 0; i < 50; i++)
                {
                    long a = (long)(rnd.NextDouble() * (basis - 1)) + 1;
                    long b = (long)(rnd.NextDouble() * (basis - 1)) + 1;

                    Assert.AreEqual(BigIntegerMultiply(a, b, basis), modulator.Multiply(a, b));
                    Assert.AreEqual(BigIntegerAdd(a, b, basis), modulator.Add(a, b));
                    Assert.AreEqual(BigIntegerSubtract(a, b, basis), modulator.Subtract(a, b));

                    long invA = modulator.Invert(a);
                    Assert.AreEqual(1L, BigIntegerMultiply(a, invA, basis));
                }
            }
        }

        [Test]
        public void CombinatoricsAgainstBigInteger()
        {
            long primeBasis = 17L;
            Modulo modulator = new Modulo(primeBasis);

            for (int n = 0; n <= 30; n++)
            {
                for (int k = 0; k <= n; k++)
                {
                    Assert.AreEqual(BigIntegerChoose(n, k, primeBasis), modulator.Choose(n, k), $"Choose({n}, {k}) failed");
                    Assert.AreEqual(BigIntegerPermutations(n, k, primeBasis), modulator.Permutations(n, k), $"Permutations({n}, {k}) failed");
                }
            }
        }

        [Test]
        public void StaticMultiplyTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Modulo.Multiply(2, 3, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Modulo.Multiply(2, 3, -7));

            Assert.AreEqual(0L, Modulo.Multiply(5, 9, 1));
            Assert.AreEqual(6L, Modulo.Multiply(2, 3, 10));
            Assert.AreEqual(BigIntegerMultiply(-123, 456, 11), Modulo.Multiply(-123, 456, 11));
            Assert.AreEqual(BigIntegerMultiply(-1, -1, 5), Modulo.Multiply(-1, -1, 5));

            long bigMod = 1_000_000_000_039L;
            Assert.AreEqual(
                BigIntegerMultiply(900_000_000_000L, 800_000_000_000L, bigMod),
                Modulo.Multiply(900_000_000_000L, 800_000_000_000L, bigMod));

            long hugeMod = long.MaxValue - 24;
            Assert.AreEqual(
                BigIntegerMultiply(long.MaxValue, long.MaxValue - 1, hugeMod),
                Modulo.Multiply(long.MaxValue, long.MaxValue - 1, hugeMod));
        }

        [Test]
        public void PowTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Modulo.Pow(2, 3, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Modulo.Pow(2, -1, 7));

            Assert.AreEqual(0L, Modulo.Pow(5, 3, 1));
            Assert.AreEqual(0L, Modulo.Pow(0, 0, 1));
            Assert.AreEqual(1L, Modulo.Pow(0, 0, 7));
            Assert.AreEqual(1L, Modulo.Pow(5, 0, 7));
            Assert.AreEqual(32L, Modulo.Pow(2, 5, 100));
            Assert.AreEqual(BigIntegerMod(BigInteger.ModPow(-3, 5, 11), 11), Modulo.Pow(-3, 5, 11));

            long largePrime = 1_000_000_000_039L;
            Modulo mod = new Modulo(largePrime);
            Assert.AreEqual((long)BigInteger.ModPow(987_654_321_012L, 123_456_789L, largePrime), mod.Pow(987_654_321_012L, 123_456_789L));
            Assert.AreEqual(1L, mod.Pow(42, largePrime - 1));
        }

        [Test]
        public void PrecomputeFactorialsAndLargePrimeTest()
        {
            long p1 = 1_000_003L;
            Modulo m1 = new Modulo(p1);
            m1.PrecomputeFactorials(1000);

            for (int i = 0; i <= 25; i++)
                Assert.AreEqual(1L, m1.Multiply(m1.Factorial(i), m1.InverseFactorial(i)));

            Assert.AreEqual(BigIntegerChoose(500, 123, p1), m1.Choose(500, 123));
            Assert.AreEqual(BigIntegerPermutations(500, 20, p1), m1.Permutations(500, 20));
            Assert.AreEqual(0L, m1.Choose(10, 12));

            // Prime > int.MaxValue previously overflowed int i * i in private IsPrime
            long bigPrime = 4_294_967_291L;
            Modulo mBig = new Modulo(bigPrime);
            mBig.PrecomputeFactorials(50);
            Assert.AreEqual(BigIntegerChoose(45, 17, bigPrime), mBig.Choose(45, 17));
            Assert.AreEqual(BigIntegerPermutations(45, 17, bigPrime), mBig.Permutations(45, 17));
            Assert.AreEqual(1L, mBig.Multiply(mBig.Factorial(45), mBig.InverseFactorial(45)));

            // Large prime 10^9+7 with Lucas + precompute
            long p2 = 1_000_000_007L;
            Modulo m2 = new Modulo(p2);
            m2.PrecomputeFactorials(100);
            Assert.AreEqual(BigIntegerChoose(100, 37, p2), m2.Choose(100, 37));

            // Non-prime basis rejects InverseFactorial
            Modulo comp = new Modulo(10);
            comp.PrecomputeFactorials(5);
            Assert.Throws<InvalidOperationException>(() => comp.InverseFactorial(2));
        }

        [Test]
        public void MinStepInRangeTest()
        {
            // Exhaustive comparison against brute force over varied moduli, start offsets, steps, and wrapped intervals.
            foreach (long m in new[] { 1L, 7L, 12L, 29L, 60L })
            {
                foreach (long start in new[] { -m - 1, 0L, 1L, m / 2, m - 1 })
                {
                    foreach (long step in new[] { -m - 2, 0L, 1L, 4L, m / 2 + 1, m - 1, 2 * m + 3 })
                    {
                        for (long l = 0; l < m; l += Math.Max(1L, m / 7))
                        {
                            for (long h = 0; h < m; h += Math.Max(1L, m / 7))
                            {
                                long? brute = null;
                                for (long k = 0; k <= m; k++)
                                {
                                    long val = BigIntegerMod(new BigInteger(start) + k * step, m);
                                    bool hit = l <= h ? (val >= l && val <= h) : (val >= l || val <= h);
                                    if (hit)
                                    {
                                        brute = k;
                                        break;
                                    }
                                }

                                Assert.AreEqual(brute, Modulo.MinStepInRange(m, start, step, l, h));
                                Assert.AreEqual(
                                    brute.HasValue ? new BigInteger(brute.Value) : (BigInteger?)null,
                                    Modulo.MinStepInRange((BigInteger)m, (BigInteger)start, (BigInteger)step, (BigInteger)l, (BigInteger)h));
                            }
                        }
                    }
                }
            }

            // Large 64-bit modulus without overflow.
            long bigM = 4_000_000_000_000_000_001L;
            long bigS = 1_234_567_890_123_456_789L;
            long kExpected = 987_654_321L;
            long target = (long)((new BigInteger(kExpected) * bigS) % bigM);
            long? bigRes = Modulo.MinStepInRange(bigM, 0L, bigS, target, target);
            Assert.IsTrue(bigRes.HasValue && bigRes.Value <= kExpected);
            Assert.AreEqual(target, (long)((new BigInteger(bigRes!.Value) * bigS) % bigM));

            // 100-digit BigInteger case.
            BigInteger pow100 = BigInteger.Pow(10, 100) + 7;
            BigInteger bStep = BigInteger.Pow(10, 99) + 123456789;
            BigInteger kTrue = 1234567890123456789L;
            BigInteger bTarget = (kTrue * bStep) % pow100;
            BigInteger? bRes = Modulo.MinStepInRange(pow100, BigInteger.Zero, bStep, bTarget, bTarget);
            Assert.IsTrue(bRes.HasValue && bRes.Value <= kTrue);
            Assert.AreEqual(bTarget, (bRes!.Value * bStep) % pow100);

            Assert.Throws<ArgumentOutOfRangeException>(() => Modulo.MinStepInRange(0L, 1L, 0L, 0L));
            Assert.Throws<ArgumentOutOfRangeException>(() => Modulo.MinStepInRange(5L, 1L, 5L, 4L));
        }

        private static long BigIntegerMod(BigInteger value, long basis)
        {
            BigInteger mod = value % basis;
            if (mod < 0) mod += basis;
            return (long)mod;
        }

        private static long BigIntegerMultiply(long a, long b, long basis)
        {
            return BigIntegerMod(new BigInteger(a) * new BigInteger(b), basis);
        }

        private static long BigIntegerAdd(long a, long b, long basis)
        {
            return BigIntegerMod(new BigInteger(a) + new BigInteger(b), basis);
        }

        private static long BigIntegerSubtract(long a, long b, long basis)
        {
            return BigIntegerMod(new BigInteger(a) - new BigInteger(b), basis);
        }

        private static long BigIntegerChoose(long n, long k, long basis)
        {
            if (k < 0 || n < k) return 0;
            if (k == 0) return 1;
            BigInteger num = 1;
            BigInteger den = 1;
            for (long i = 1; i <= k; i++)
            {
                num *= (n - i + 1);
                den *= i;
            }
            BigInteger comb = num / den;
            return BigIntegerMod(comb, basis);
        }

        private static long BigIntegerPermutations(long n, long k, long basis)
        {
            if (k < 0 || n < k) return 0;
            if (k == 0) return 1;
            BigInteger res = 1;
            for (long i = 0; i < k; i++)
            {
                res *= (n - i);
            }
            return BigIntegerMod(res, basis);
        }
    }
}