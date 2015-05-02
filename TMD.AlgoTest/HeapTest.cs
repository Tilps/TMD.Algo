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
    public class HeapTest
    {
        [Test]
        public void Basic()
        {
            List<double> baseline = new List<double>();
            Heap<double> heap = new Heap<double>();
            Random rnd = new Random(1);
            for (int i = 0; i < 1000; i++)
            {
                baseline.Add(rnd.NextDouble());
                heap.Add(baseline[i]);
            }
            for (int i = 0; i < 100; i++)
            {
                double choice = baseline[rnd.Next(baseline.Count)];
                baseline.Remove(choice);
                heap.Remove(choice);
                heap.___TESTValidateTEST();
            }
            Heap<double> heap2 = new Heap<double>(heap);
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.IsTrue(heap.Contains(baseline[i]));
                Assert.IsTrue(heap2.Contains(baseline[i]));
            }
            baseline.Sort();
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[baseline.Count - i - 1], heap.Front);
                Assert.AreEqual(baseline[baseline.Count - i - 1], heap2.Front);
                Assert.AreEqual(baseline[baseline.Count - i - 1], heap.PopFront());
                Assert.AreEqual(baseline[baseline.Count - i - 1], heap2.PopFront());
            }
        }
    }
}