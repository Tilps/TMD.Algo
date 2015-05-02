#region License

/*
Copyright (c) 2015, the TMD.Algo authors.
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
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2008
{
    [TestFixture]
    public class R1CTest : BaseGCJTest
    {
        private Func<long> Q1Solver(GCJTestCase test)
        {
            int P, K, L;
            test.Get(out P, out K, out L);
            int[] freqs;
            test.Get(out freqs);
            return () => Q1Solver(P, K, freqs);
        }

        private long Q1Solver(int P, int K, int[] freqs)
        {
            Array.Sort(freqs);
            Array.Reverse(freqs);
            long total = 0;
            for (int i = 0; i < freqs.Length; i++)
            {
                total += (1 + i/K)*freqs[i];
            }
            return total;
        }

        [Test]
        public void Q1Sample()
        {
            string input =
                #region SampleInput

                @"2
3 2 6
8 2 5 2 4 9
3 9 26
1 1 1 100 100 1 1 1 1 1 1 1 1 1 1 1 1 10 11 11 11 11 1 1 1 100";

            #endregion

            string expectedOutput = @"Case #1: 47
Case #2: 397";

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

        private Func<long> Q2Solver(GCJTestCase test)
        {
            string digits;
            test.Get(out digits);
            return () => Q2Solver(digits.Select(c => int.Parse("" + c)).ToArray());
        }

        private long Q2Solver(int[] digits)
        {
            long[,] counts = new long[digits.Length + 1, 210];
            counts[0, 0] = 1;
            for (int i = 0; i < digits.Length; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    BigInteger value = 0;
                    for (int k = i - j; k <= i; k++)
                    {
                        value *= 10;
                        value += digits[k];
                    }
                    int mod = (int)(value%210);
                    for (int k = 0; k < 210; k++)
                    {
                        int net = (mod + k)%210;
                        counts[i + 1, net] += counts[i - j, k];
                        if (i != j)
                        {
                            net = (210 + k - mod)%210;
                            counts[i + 1, net] += counts[i - j, k];
                        }
                    }
                }
            }
            return
                Enumerable.Range(0, 210)
                    .Where(a => (a%2 == 0 || a%3 == 0 || a%5 == 0 || a%7 == 0))
                    .Sum(i => counts[digits.Length, i]);
        }

        [Test]
        public void Q2Sample()
        {
            string input =
                #region SampleInput

                @"4
1
9
011
12345";

            #endregion

            string expectedOutput = @"Case #1: 0
Case #2: 1
Case #3: 6
Case #4: 64";

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
            SlowTest(Q2Solver);
        }

        private Func<long> Q3Solver(GCJTestCase test)
        {
            long n, m, X, Y, Z;
            test.Get(out n, out m, out X, out Y, out Z);
            long[] gens;
            test.GetLines((int)m, out gens);
            return () => Q3Solver(n, gens, X, Y, Z);
        }

        private long Q3Solver(long n, long[] gens, long X, long Y, long Z)
        {
            List<long> values = new List<long>();
            for (int i = 0; i < n; i++)
            {
                values.Add(gens[i%gens.Length]);
                gens[i%gens.Length] = (X*gens[i%gens.Length] + Y*(i + 1))%Z;
            }
            int[] newValues = values.Relabel();
            AccumulatorTreeList<long> sums = new AccumulatorTreeList<long>(new LongAdder());
            int max = newValues.Max();
            for (int i = 0; i <= max; i++)
            {
                sums.Add(0);
            }
            for (int i = 0; i < newValues.Length; i++)
            {
                int x = newValues[i];
                long miniTotal = sums.GetSum(x);
                sums[x] = (miniTotal + 1)%1000000007;
            }
            return sums.GetSum(sums.Count - 1)%1000000007;
        }


        [Test]
        public void Q3Sample()
        {
            string input =
                #region SampleInput

                @"2
5 5 0 0 5
1
2
1
2
3
6 2 2 1000000000 6
1
2";

            #endregion

            string expectedOutput = @"Case #1: 15
Case #2: 13";

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
            SlowTest(Q3Solver);
        }
    }
}