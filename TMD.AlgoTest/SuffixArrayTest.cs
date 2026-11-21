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
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class SuffixArrayTest
    {
        private static void VerifySuffixArray(string s)
        {
            SuffixArray sa = SuffixArray.Build(s);
            int n = s.Length;
            Assert.AreEqual(n, sa.Length);
            Assert.AreEqual(n, sa.Order.Count);
            Assert.AreEqual(n, sa.Rank.Count);
            Assert.AreEqual(Math.Max(0, n - 1), sa.Lcp.Count);

            for (int i = 0; i < n; i++)
                Assert.AreEqual(i, sa.Order[sa.Rank[i]]);

            for (int i = 0; i < n - 1; i++)
            {
                string a = s[sa.Order[i]..];
                string b = s[sa.Order[i + 1]..];
                Assert.Less(string.CompareOrdinal(a, b), 0, $"Order violation at {i} for '{s}'");

                int expectedLcp = 0;
                while (expectedLcp < a.Length && expectedLcp < b.Length && a[expectedLcp] == b[expectedLcp])
                    expectedLcp++;
                Assert.AreEqual(expectedLcp, sa.Lcp[i], $"LCP mismatch at {i} for '{s}'");
            }

            HashSet<string> distinct = [];
            for (int i = 0; i < n; i++)
            for (int len = 1; i + len <= n; len++)
                distinct.Add(s.Substring(i, len));
            Assert.AreEqual(distinct.Count, sa.CountDistinctSubstrings());
        }

        [Test]
        public void EdgeAndStandardStrings()
        {
            VerifySuffixArray("");
            VerifySuffixArray("a");
            VerifySuffixArray("aaaa");
            VerifySuffixArray("banana");
            VerifySuffixArray("abracadabra");
            VerifySuffixArray("mississippi");
            VerifySuffixArray("tbilcwhafiparmdmotcaaloth");
            VerifySuffixArray("\u03b1\u03b2\u03b1\u03b3\u03b1");
        }
    }
}
