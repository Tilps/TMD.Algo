#region License

/*
Copyright (c) 2026, the TMD.Algo authors.
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
    public class BigFractionTest
    {
        [Test]
        public void NormalizationAndEdgeCases()
        {
            Assert.AreEqual(BigInteger.Zero, BigFraction.Zero.Numerator);
            Assert.AreEqual(BigInteger.One, BigFraction.Zero.Denominator);
            Assert.IsTrue(BigFraction.Zero.IsZero);

            Assert.Throws<DivideByZeroException>(() => _ = new BigFraction(1, 0));
            Assert.Throws<DivideByZeroException>(() => _ = BigFraction.One / BigFraction.Zero);

            BigFraction zero = new(0, -123456789);
            Assert.AreEqual(BigFraction.Zero, zero);
            Assert.AreEqual("0/1", zero.ToString());

            BigFraction neg = new(6, -15);
            Assert.AreEqual(new BigInteger(-2), neg.Numerator);
            Assert.AreEqual(new BigInteger(5), neg.Denominator);
            Assert.AreEqual(-1, neg.Sign);
            Assert.AreEqual(new BigFraction(2, 5), neg.Abs());
            Assert.AreEqual(new BigFraction(2, 5), -neg);
        }

        [Test]
        public void ArbitraryPrecisionArithmeticAndComparison()
        {
            BigInteger p100 = BigInteger.Pow(10, 100);
            BigFraction a = new(p100 + 1, 3);
            BigFraction b = new(p100 - 1, 6);

            BigFraction sum = a + b;
            BigFraction expectedSum = new(3 * p100 + 1, 6);
            Assert.AreEqual(expectedSum, sum);

            BigFraction diff = a - b;
            BigFraction expectedDiff = new(p100 + 3, 6);
            Assert.AreEqual(expectedDiff, diff);

            BigFraction prod = new BigFraction(p100, 7) * new BigFraction(21, p100);
            Assert.AreEqual(new BigFraction(3, 1), prod);

            BigFraction quot = new BigFraction(p100, 7) / new BigFraction(2 * p100, 21);
            Assert.AreEqual(new BigFraction(3, 2), quot);

            BigFraction slightlyLess = new(p100, 2 * p100 + 1);
            BigFraction half = new(1, 2);
            Assert.IsTrue(slightlyLess < half);
            Assert.IsTrue(slightlyLess <= half);
            Assert.IsTrue(half > slightlyLess);
            Assert.IsTrue(half >= slightlyLess);
            Assert.IsTrue(slightlyLess != half);
            Assert.AreEqual(new BigFraction(1, 2).GetHashCode(), new BigFraction(-2, -4).GetHashCode());
        }
    }
}
