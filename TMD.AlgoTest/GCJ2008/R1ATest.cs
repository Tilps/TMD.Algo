#region License

/*
Copyright (c) 2014, the TMD.Algo authors.
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
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2008
{
    [TestFixture]
    public class R1ATest : BaseGCJTest
    {
        private Func<long> Q1Solver(GCJTestCase test)
        {
            int n;
            test.Get(out n);
            long[] a;
            test.Get(out a);
            long[] b;
            test.Get(out b);
            return () => Q1Solver(a, b);
        }

        private long Q1Solver(long[] a, long[] b)
        {
            Array.Sort(a);
            Array.Sort(b);
            Array.Reverse(b);
            return a.Zip(b, (ai, bi) => ai*bi).Sum();
        }

        [Test]
        public void Q1Sample()
        {
            string input =
                #region SampleInput

                @"2
3
1 3 -5
-2 4 1
5
1 2 3 4 5
1 0 1 0 1";

            #endregion

            string expectedOutput = @"Case #1: -25
Case #2: 6";

            var result = GCJ.___TESTRunTestsTEST(input, Q1Solver);
            Validate(expectedOutput, result);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver);
        }

        [Test]
        public void Q1Large()
        {
            Test(Q1Solver);
        }

        private Func<object> Q2Solver(GCJTestCase test)
        {
            int n;
            test.Get(out n);
            int m;
            test.Get(out m);
            int[][] custInts;
            test.GetMatrix(m, out custInts);
            bool[,,] likes = new bool[n, m, 2];
            for (int c = 0; c < m; c++)
            {
                for (int i = 1; i < custInts[c].Length; i += 2)
                {
                    likes[custInts[c][i] - 1, c, custInts[c][i + 1]] = true;
                }
            }
            return () => Q2Solver(n, m, likes);
        }

        private object Q2Solver(int n, int m, bool[,,] likes)
        {
            int[] satisCount = new int[m];
            int[] malts = new int[m];
            for (int i = 0; i < m; i++)
            {
                malts[i] = -1;
                for (int j = 0; j < n; j++)
                {
                    if (likes[j, i, 1])
                        malts[i] = j;
                }
            }
            LookupQueue<int> toMalt = new LookupQueue<int>();
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (likes[j, i, 0])
                        satisCount[i]++;
                }
                if (satisCount[i] == 0)
                {
                    int malt = malts[i];
                    if (malt == -1) return "IMPOSSIBLE";
                    toMalt.TryEnqueue(malt);
                }
            }
            HashSet<int> malted = new HashSet<int>();
            int nextMalt;
            while (toMalt.TryDequeue(out nextMalt))
            {
                malted.Add(nextMalt);
                for (int i = 0; i < m; i++)
                {
                    if (likes[nextMalt, i, 1])
                    {
                        satisCount[i]++;
                    }
                    else if (likes[nextMalt, i, 0])
                    {
                        satisCount[i]--;
                        if (satisCount[i] == 0)
                        {
                            int malt = malts[i];
                            if (malt == -1) return "IMPOSSIBLE";
                            toMalt.TryEnqueue(malt);
                        }
                    }
                }
            }
            return Enumerable.Range(0, n).Select(i => malted.Contains(i) ? 1 : 0);
        }

        [Test]
        public void Q2Sample()
        {
            string input =
                #region SampleInput

                @"2
5
3
1 1 1
2 1 0 2 0
1 5 0
1
2
1 1 0
1 1 1";

            #endregion

            string expectedOutput = @"Case #1: 1 0 0 0 0
Case #2: IMPOSSIBLE";

            var result = GCJ.___TESTRunTestsTEST(input, Q2Solver);
            Validate(expectedOutput, result);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver);
        }

        private Func<string> Q3Solver(GCJTestCase test)
        {
            int n;
            test.Get(out n);
            return () => Q3Solver(n);
        }

        private static int[] Multiply(int[] a, int[] b)
        {
            int[] result = new int[2];
            result[0] = (a[0]*b[0] + 5*a[1]*b[1])%1000;
            result[1] = (a[0]*b[1] + a[1]*b[0])%1000;
            return result;
        }

        private class Root5Pair
        {
            public int[] Value;

            public override bool Equals(object obj)
            {
                Root5Pair other = (Root5Pair)obj;
                return Value[0] == other.Value[0] && Value[1] == other.Value[1];
            }

            public override int GetHashCode()
            {
                return Value[0].GetHashCode()*33 ^ Value[1].GetHashCode();
            }
        }

        private static Root5Pair Multiply(Root5Pair a, Root5Pair b)
        {
            return new Root5Pair() {Value = Multiply(a.Value, b.Value)};
        }

        private string Q3Solver(int n)
        {
            // Fast log (n) multiplies implementation.
            int[] result = MathFormulas.FastExponent(new[] {3, 1}, n, Multiply);
            // Slower mod size squared implementation O(1) multiplies, but with larger constant than log(n) will ever give in practice.
            // Interestingly it isn't that much worse because the cycle length is only ~500, well short of the maximum cycle length of 1 million.
            Root5Pair value = new Root5Pair() {Value = new[] {3, 1}};
            Root5Pair result2 = Pattern.FindInPattern(value, pair => Multiply(value, pair), n - 1);
            if (result2.Value[0] != result[0] || result2.Value[1] != result[1])
                throw new InvalidOperationException("Fast Exponent and find in pattern disagree.");
            return $"{(result[0]*2 - 1)%1000:000}";
        }


        [Test]
        public void Q3Sample()
        {
            string input =
                #region SampleInput

                @"2
5
2";

            #endregion

            string expectedOutput = @"Case #1: 935
Case #2: 027";

            var result = GCJ.___TESTRunTestsTEST(input, Q3Solver);
            Validate(expectedOutput, result);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Large()
        {
            Test(Q3Solver);
        }
    }
}