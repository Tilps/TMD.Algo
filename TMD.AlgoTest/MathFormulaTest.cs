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
    }
}