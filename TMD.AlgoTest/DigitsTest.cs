#region License

/*
Copyright (c) 2016, the TMD.Algo authors.
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
using NUnit.Framework;
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class DigitsTest
    {
        [Test]
        public void Basic()
        {
            unchecked
            {
                Random rnd = new Random();
                int[] cornerCases = new[]
                {
                    0, 1, -1, int.MaxValue, int.MinValue, 0x33333333, 0x55555555, 0x0F0F0F0F, 0x00FF00FF, 0x0000FFFF,
                    (int)0xAAAAAAAA, (int)0xCCCCCCCC, (int)0xF0F0F0F0, (int)0xFF00FF00, (int)0xFFFF0000
                };
                List<int> testCases = new List<int>(cornerCases);
                for (int i = 0; i < 1000; i++)
                {
                    // This never adds int.MaxValue again, but thats okay because we've already included it.
                    testCases.Add(rnd.Next(int.MinValue, int.MaxValue));
                }
                Verify(testCases);
            }
        }

        private void Verify(List<int> testCases)
        {
            for (int i = 0; i < testCases.Count; i++)
            {
                for (int j = 2; j < 200; j++)
                {
                    if (testCases[i] >= 0)
                    {
                        Assert.AreEqual(testCases[i], (int)testCases[i].ToDigits(j).ToValueAsDigits(j));
                    }
                    else
                    {
                        Assert.Throws<ArgumentException>(() => { testCases[i].ToDigits(j).ToValueAsDigits(j); });
                    }
                }
                if (testCases[i] > 2)
                {
                    // Create some base 3 digits.
                    int[] digits = testCases[i].ToDigits(3).ToArray();
                    // Base 4 of same digits is greater than Base 3 since original value is greater than 2.
                    Assert.Greater(digits.ToValueAsDigits(4), digits.ToValueAsDigits(3));
                    // Digit collection ToBigInteger should be same as ToValueAsDigits for the digitBase provided to constructor.
                    DigitCollection a = new DigitCollection(digits, 4);
                    Assert.AreEqual(digits.ToValueAsDigits(4), a.ToBigInteger());
                    // Switch base should have same digits, just different base, thus same value as original digits for that base.
                    DigitCollection b = a.SwitchBase(3);
                    Assert.AreEqual(digits.ToValueAsDigits(3), b.ToBigInteger());
                }
            }
        }

    }
}
