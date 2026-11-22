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
    public class PatternTest
    {
        [Test]
        public void PrefixFunctionAndZArrayEdgeAndSampleCases()
        {
            Assert.IsEmpty(Pattern.ComputePrefixFunction(ReadOnlySpan<char>.Empty));
            Assert.IsEmpty(Pattern.ComputeZArray(ReadOnlySpan<char>.Empty));

            CollectionAssert.AreEqual(new[] { 0 }, Pattern.ComputePrefixFunction("A"));
            CollectionAssert.AreEqual(new[] { 1 }, Pattern.ComputeZArray("A"));

            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, Pattern.ComputePrefixFunction("AAAAA"));
            CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 }, Pattern.ComputeZArray("AAAAA"));

            CollectionAssert.AreEqual(new[] { 0, 0, 1, 2, 3, 4 }, Pattern.ComputePrefixFunction("ABABAB"));
            CollectionAssert.AreEqual(new[] { 6, 0, 4, 0, 2, 0 }, Pattern.ComputeZArray("ABABAB"));

            CollectionAssert.AreEqual(new[] { 0, 0, 1, 0, 1, 2, 3 }, Pattern.ComputePrefixFunction("ABACABA"));
            CollectionAssert.AreEqual(new[] { 7, 0, 1, 0, 3, 0, 1 }, Pattern.ComputeZArray("ABACABA"));

            int[] ints = [1, 2, 1, 2, 1];
            CollectionAssert.AreEqual(new[] { 0, 0, 1, 2, 3 }, Pattern.ComputePrefixFunction<int>(ints));
            CollectionAssert.AreEqual(new[] { 5, 0, 3, 0, 1 }, Pattern.ComputeZArray<int>(ints));
        }

        [Test]
        public void PrefixFunctionAndZArrayRandomizedCrossCheck()
        {
            Random rng = new(201501);
            char[] buf = new char[40];
            for (int trial = 0; trial < 200; trial++)
            {
                int len = rng.Next(1, buf.Length + 1);
                int alphabet = rng.Next(2, 5);
                for (int i = 0; i < len; i++)
                    buf[i] = (char)('A' + rng.Next(alphabet));

                ReadOnlySpan<char> span = buf.AsSpan(0, len);
                int[] pi = Pattern.ComputePrefixFunction(span);
                int[] z = Pattern.ComputeZArray(span);

                Assert.AreEqual(len, pi.Length);
                Assert.AreEqual(len, z.Length);

                for (int i = 0; i < len; i++)
                {
                    int expectedPi = 0;
                    for (int k = i; k >= 1; k--)
                    {
                        if (span[..k].SequenceEqual(span.Slice(i + 1 - k, k)))
                        {
                            expectedPi = k;
                            break;
                        }
                    }
                    Assert.AreEqual(expectedPi, pi[i]);

                    int expectedZ = 0;
                    while (i + expectedZ < len && span[expectedZ] == span[i + expectedZ])
                        expectedZ++;
                    Assert.AreEqual(expectedZ, z[i]);
                }
            }
        }
    }
}
