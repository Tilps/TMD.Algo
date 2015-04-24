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
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class SpecialtySortTest
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
            List<int> baseline = new List<int>(values);
            baseline.Sort();
            int[] counting = new List<int>(values).CountingSort(i => i, 0, 999);
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], counting[i]);
            }
            int[] radix = new List<int>(values).RadixSort(delegate(int i, int j) { return (i >> (j * 4)) & 0xF; }, 0, 15, 3);
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], radix[i]);
            }
            List<int> insertion = new List<int>(values);
            insertion.InsertionSort();
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], insertion[i]);
            }
            List<int> merge = new List<int>(values);
            merge.MergeSort();
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], merge[i]);
            }
            List<int> merge2 = new List<int>(values);
            merge2.MergeSort2();
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], merge2[i]);
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
            List<int> baseline = new List<int>(values);
            baseline.Sort();
            int[] counting = new List<int>(values).CountingSort(i => i, 0, 999);
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], counting[i]);
            }
            int[] radix = new List<int>(values).RadixSort(delegate(int i, int j) { return (i >> (j * 4)) & 0xF; }, 0, 15, 3);
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], radix[i]);
            }
            List<int> insertion = new List<int>(values);
            insertion.InsertionSort();
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], insertion[i]);
            }
            List<int> merge = new List<int>(values);
            merge.MergeSort();
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], merge[i]);
            }
            List<int> merge2 = new List<int>(values);
            merge2.MergeSort2();
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], merge2[i]);
            }

        }

        [Test]
        public void Buckets()
        {
            Random rnd = new Random();
            List<double> values = new List<double>();

            for (int i = 0; i < 1000; i++)
            {
                values.Add(rnd.NextDouble());
            }
            List<double> baseline = new List<double>(values);
            baseline.Sort();
            List<double> buckets = new List<double>(values).BucketSort(delegate(double i, int j) { return (int)Math.Floor(i * j); });
            for (int i = 0; i < baseline.Count; i++)
            {
                Assert.AreEqual(baseline[i], buckets[i]);
            }

        }
    }
}
