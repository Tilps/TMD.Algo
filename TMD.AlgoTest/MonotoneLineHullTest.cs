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
using System.Collections.Generic;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class MonotoneLineHullTest
    {
        [Test]
        public void UpperAndLowerEnvelopesMatchBruteForce()
        {
            Random rng = new(201406);
            List<(double M, double B)> maxLines = [];
            List<(double M, double B)> minLines = [];

            double curM = -20.0;
            for (int i = 0; i < 60; i++)
            {
                // Include occasional duplicate slopes to test tie replacement.
                if (i % 7 != 0)
                    curM += 0.1 + rng.NextDouble() * 1.5;
                double b = (rng.NextDouble() - 0.5) * 40.0;
                maxLines.Add((curM, b));
                minLines.Add((-curM, b));
            }

            MonotoneLineHull<double> maxHull = new(maximize: true);
            MonotoneLineHull<double> minHull = new(maximize: false);
            foreach (var (m, b) in maxLines)
                maxHull.AddLine(m, b);
            foreach (var (m, b) in minLines)
                minHull.AddLine(m, b);

            int maxPtr = 0;
            int minPtr = 0;
            for (int q = -200; q <= 200; q++)
            {
                double x = q * 0.25;
                double expMax = double.NegativeInfinity;
                foreach (var (m, b) in maxLines)
                    expMax = Math.Max(expMax, m * x + b);

                double expMin = double.PositiveInfinity;
                foreach (var (m, b) in minLines)
                    expMin = Math.Min(expMin, m * x + b);

                Assert.AreEqual(expMax, maxHull.Query(x), 1e-11);
                Assert.AreEqual(expMax, maxHull.QueryMonotone(x, ref maxPtr), 1e-11);
                Assert.AreEqual(expMin, minHull.Query(x), 1e-11);
                Assert.AreEqual(expMin, minHull.QueryMonotone(x, ref minPtr), 1e-11);
            }

            // Decimal support & Clear().
            MonotoneLineHull<decimal> decHull = new(maximize: true);
            decHull.AddLine(0m, 0.5m);
            decHull.AddLine(1m, 0.7m);
            decHull.AddLine(2m, 0.2m);
            Assert.AreEqual(0.5m, decHull.Query(-1m));
            Assert.AreEqual(0.7m, decHull.Query(0m));
            Assert.AreEqual(2.2m, decHull.Query(1m));

            decHull.Clear();
            Assert.AreEqual(0, decHull.Count);
            Assert.Throws<InvalidOperationException>(() => decHull.Query(0m));
            int ptr = 0;
            Assert.Throws<InvalidOperationException>(() => decHull.QueryMonotone(0m, ref ptr));

            // Slope ordering validation.
            Assert.Throws<ArgumentException>(() => maxHull.AddLine(-1000.0, 0.0));
            Assert.Throws<ArgumentException>(() => minHull.AddLine(1000.0, 0.0));
        }
    }
}
