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
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class BitTricksTest
    {

        [Test]
        public void Basic()
        {
            unchecked
            {
                Random rnd = new Random();
                int[] cornerCases = new[] { 0, 1, -1, int.MaxValue, int.MinValue, 0x33333333, 0x55555555, 0x0F0F0F0F, 0x00FF00FF, 0x0000FFFF, (int)0xAAAAAAAA, (int)0xCCCCCCCC, (int)0xF0F0F0F0, (int)0xFF00FF00, (int)0xFFFF0000 };
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
                VerifyPopCount(testCases[i]);
                VerifyHighbitPos(testCases[i]);
                VerifyLowbitPos(testCases[i]);
                VerifyNextCombination(testCases[i]);
            }
        }

        private void VerifyNextCombination(int p)
        {
            // 0 is the only combination with 0 bits.
            if (p == 0)
                return;
            // These have no next combination, the are the last combination.
            for (int i = 0; i < 32; i++)
            {
                if (p == ((-1) << i))
                    return;
            }
            Assert.AreEqual(BitTricks.PopulationCount(p), BitTricks.PopulationCount(BitTricks.NextCombination(p)));
            Assert.AreNotEqual(p, BitTricks.NextCombination(p));

        }

        private void VerifyLowbitPos(int p)
        {
            int pos = -1;
            for (int i = 0; i < 32; i++)
            {
                if (((1 << i) & p) != 0)
                {
                    pos = i;
                    break;
                }
            }
            Assert.AreEqual(pos, BitTricks.LeastSignificantSetBitPos(p));
        }

        private void VerifyHighbitPos(int p)
        {
            int pos = -1;
            for (int i = 31; i >= 0; i--)
            {
                if (((1 << i) & p) != 0)
                {
                    pos = i;
                    break;
                }
            }
            Assert.AreEqual(pos, BitTricks.MostSignificantSetBitPos(p));
        }

        private void VerifyPopCount(int p)
        {
            int count = 0;
            for (int i = 0; i < 32; i++)
            {
                if (((1 << i) & p) != 0)
                    count++;
            }
            Assert.AreEqual(count, BitTricks.PopulationCount(p));
        }
    }
}
