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