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

        [Test]
        public void LowerAndUpperBoundTests()
        {
            Assert.AreEqual(0, SpecialtySelect.LowerBound<int>(ReadOnlySpan<int>.Empty, 5));
            Assert.AreEqual(0, SpecialtySelect.UpperBound<int>(ReadOnlySpan<int>.Empty, 5));

            int[] values = [1, 2, 2, 2, 5, 7, 7, 10];
            Assert.AreEqual(0, SpecialtySelect.LowerBound<int>(values, 0));
            Assert.AreEqual(0, SpecialtySelect.UpperBound<int>(values, 0));

            Assert.AreEqual(1, SpecialtySelect.LowerBound<int>(values, 2));
            Assert.AreEqual(4, SpecialtySelect.UpperBound<int>(values, 2));

            Assert.AreEqual(4, SpecialtySelect.LowerBound<int>(values, 3));
            Assert.AreEqual(4, SpecialtySelect.UpperBound<int>(values, 3));

            Assert.AreEqual(5, SpecialtySelect.LowerBound<int>(values, 7));
            Assert.AreEqual(7, SpecialtySelect.UpperBound<int>(values, 7));

            Assert.AreEqual(8, SpecialtySelect.LowerBound<int>(values, 11));
            Assert.AreEqual(8, SpecialtySelect.UpperBound<int>(values, 11));

            // Custom descending comparer.
            int[] descValues = [10, 7, 7, 5, 2, 2, 1];
            Comparer<int> desc = Comparer<int>.Create((a, b) => b.CompareTo(a));
            Assert.AreEqual(1, SpecialtySelect.LowerBound<int>(descValues, 7, desc));
            Assert.AreEqual(3, SpecialtySelect.UpperBound<int>(descValues, 7, desc));
            Assert.AreEqual(0, SpecialtySelect.LowerBound<int>(descValues, 20, desc));
            Assert.AreEqual(7, SpecialtySelect.UpperBound<int>(descValues, 0, desc));
        }

        [Test]
        public void FindTransitionTests()
        {
            // long overload: false -> true and true -> false, negative ranges, and boundary transitions.
            Assert.AreEqual(-1L, SpecialtySelect.FindTransition(-1L, 0L, v => v >= 0));
            Assert.AreEqual(0L, SpecialtySelect.FindTransition(-1L, 10L, v => v >= 1));
            Assert.AreEqual(9L, SpecialtySelect.FindTransition(-1L, 10L, v => v >= 10));
            Assert.AreEqual(-38L, SpecialtySelect.FindTransition(-100L, 100L, v => v >= -37));
            Assert.AreEqual(42L, SpecialtySelect.FindTransition(-100L, 100L, v => v <= 42));
            Assert.Throws<ArgumentException>(() => SpecialtySelect.FindTransition(0L, 10L, _ => true));
            Assert.Throws<ArgumentException>(() => SpecialtySelect.FindTransition(5L, 2L, v => v > 3));

            // double overload without explicit delta.
            double t1 = SpecialtySelect.FindTransition(-10.0, 10.0, v => v >= 3.25);
            Assert.AreEqual(3.25, t1, 1e-12);
            double t2 = SpecialtySelect.FindTransition(-10.0, 10.0, v => v <= -1.5);
            Assert.AreEqual(-1.5, t2, 1e-12);
            Assert.Throws<ArgumentException>(() => SpecialtySelect.FindTransition(0.0, 5.0, _ => false));

            // double overload with explicit delta.
            double d1 = SpecialtySelect.FindTransition(0.0, 10.0, 1e-6, v => v >= 4.75);
            Assert.AreEqual(4.75, d1, 1e-5);
            double d2 = SpecialtySelect.FindTransition(-5.0, 5.0, 1e-6, v => v < -0.5);
            Assert.AreEqual(-0.5, d2, 1e-5);
            Assert.Throws<ArgumentException>(() => SpecialtySelect.FindTransition(0.0, 5.0, 1e-6, _ => true));
        }
    }
}