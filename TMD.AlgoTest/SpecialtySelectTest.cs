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
using NUnit.Framework;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class SpecialtySelectTest
    {
        [Test]
        public void Simple()
        {
            Random rnd = new Random();
            List<int> values = new List<int>();

            for (int i = 0; i < 1000; i++)
            {
                values.Add(i);
            }
            for (int i = 0; i < 1000; i++)
            {
                int j = rnd.Next(values.Count);
                if (j == i)
                    continue;
                int tmp = values[j];
                values[j] = values[i];
                values[i] = tmp;
            }
            Assert.AreEqual(0, values.FindMinimum());
            Assert.AreEqual(999, values.FindMaximum());
            for (int i = 0; i < 1000; i++)
            {
                List<int> valuesCopy = new List<int>(values);
                Assert.AreEqual(i, valuesCopy.SelectRandomizedInPlace(i));
            }
        }

        [Test]
        public void Duplicates()
        {
            Random rnd = new Random();
            List<int> values = new List<int>();

            for (int i = 0; i < 1000; i++)
            {
                values.Add(i/2);
            }
            for (int i = 0; i < 1000; i++)
            {
                int j = rnd.Next(values.Count);
                if (j == i)
                    continue;
                int tmp = values[j];
                values[j] = values[i];
                values[i] = tmp;
            }
            Assert.AreEqual(0, values.FindMinimum());
            Assert.AreEqual(499, values.FindMaximum());
            for (int i = 0; i < 1000; i++)
            {
                List<int> valuesCopy = new List<int>(values);
                Assert.AreEqual(i/2, valuesCopy.SelectRandomizedInPlace(i));
            }
        }
    }
}