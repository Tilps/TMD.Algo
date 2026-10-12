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

            Assert.AreEqual(2, modulator.Permutations(8, 2));
            Assert.AreEqual(2, modulator.Choose(6, 3));
        }

        [Test]
        public void LargeNumbers()
        {
            long basis = 1_000_000_007L;
            Modulo modulator = new Modulo(basis);

            long a = 1_000_000_000L;
            long b = 1_000_000_000L;
            // a * b = 10^18, under mod 10^9+7:
            // 10^18 % 10^9+7 = 49
            long expected = (long)(((UInt128)(ulong)a * (UInt128)(ulong)b) % (UInt128)(ulong)basis);
            Assert.AreEqual(expected, modulator.Multiply(a, b));

            long bigMod = 1_000_000_000_039L; // prime > 10^12
            Modulo bigModulator = new Modulo(bigMod);
            long x = 900_000_000_000L;
            long y = 800_000_000_000L;
            // x * y = 7.2 * 10^23, which drastically exceeds long.MaxValue (9.22 * 10^18)!
            long expectedBig = (long)(((UInt128)(ulong)x * (UInt128)(ulong)y) % (UInt128)(ulong)bigMod);
            Assert.AreEqual(expectedBig, bigModulator.Multiply(x, y));
        }
    }
}