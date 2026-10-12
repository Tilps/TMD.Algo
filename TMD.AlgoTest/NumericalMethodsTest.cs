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
using NUnit.Framework;
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class NumericalMethodsTest
    {
        [Test]
        public void ScalarRk4Exponential()
        {
            // y' = y, y(0) = 1 => y(x) = e^x
            double result = NumericalMethods.RungeKutta4((x, y) => y, 0.0, 1.0, 1.0, 100);
            Assert.AreEqual(Math.E, result, 1e-9);
        }

        [Test]
        public void VectorRk4HarmonicOscillator()
        {
            // y'' = -y with y(0) = 0, y'(0) = 1 => y(x) = sin(x), y'(x) = cos(x)
            // State vector: [y, y']
            Func<double, double[], double[]> deriv = (x, state) => new double[] { state[1], -state[0] };
            double[] result = NumericalMethods.RungeKutta4(deriv, 0.0, new double[] { 0.0, 1.0 }, Math.PI / 2.0, 200);

            Assert.AreEqual(1.0, result[0], 1e-7); // sin(pi/2) = 1
            Assert.AreEqual(0.0, result[1], 1e-7); // cos(pi/2) = 0
        }

        [Test]
        public void SimpsonIntegration()
        {
            // Integral of sin(x) from 0 to pi = 2
            double sinInt = NumericalMethods.Simpson(Math.Sin, 0.0, Math.PI, 100);
            Assert.AreEqual(2.0, sinInt, 1e-7);

            // Integral of x^2 from 0 to 1 = 1/3
            double polyInt = NumericalMethods.Simpson(x => x * x, 0.0, 1.0, 10);
            Assert.AreEqual(1.0 / 3.0, polyInt, 1e-9);
        }

        [Test]
        public void ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => NumericalMethods.RungeKutta4((Func<double, double, double>)null, 0, 0, 1, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => NumericalMethods.RungeKutta4((x, y) => y, 0, 0, 1, 0));
            Assert.Throws<ArgumentNullException>(() => NumericalMethods.RungeKutta4((Func<double, double[], double[]>)null, 0, new double[] { 0 }, 1, 10));
            Assert.Throws<ArgumentNullException>(() => NumericalMethods.RungeKutta4((x, y) => y, 0, null, 1, 10));
            Assert.Throws<ArgumentNullException>(() => NumericalMethods.Simpson(null, 0, 1));
        }
    }
}

