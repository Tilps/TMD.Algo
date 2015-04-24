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
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2008
{
    [TestFixture]
    public class R1BTest : BaseGCJTest
    {
        private Func<long> Q1Solver(GCJTestCase test)
        {
            long n, A, B, C, D, x0, y0, M;
            test.Get(out n, out A, out B, out C, out D, out x0, out y0, out M);
            return () => Q1Solver(n, A, B, C, D, x0, y0, M);
        }

        private long Q1Solver(long n, long A, long B, long C, long D, long x0, long y0, long M)
        {
            long[,] counts = new long[3,3];
            long X = x0, Y = y0;
            for (int i = 0; i < n; i++)
            {
                counts[X%3, Y%3]++;
                X = (A*X + B)%M;
                Y = (C*Y + D)%M;
            }
            long total = 0;
            for (int i = 0; i < 9; i++)
            {
                int a = i/3;
                int b = i%3;
                for (int j = 0; j < 9; j++)
                {
                    int c = j/3;
                    int d = j%3;
                    int e = (6  - a - c) % 3;
                    int f = (6 - b - d) % 3;
                    long count1 = counts[a, b];
                    long count2 = counts[c, d];
                    long count3 = counts[e, f];
                    if (a == c && b == d && a == e && b == f)
                    {
                        count2 = Math.Max(0, count2 - 1);
                        count3 = Math.Max(0, count3 - 2);
                    }
                    else if (a == c && b == d)
                    {
                        count2 = Math.Max(0, count2 - 1);
                    }
                    else if (a == e && b == f || c == e && d == f)
                    {
                        count3 = Math.Max(0, count3 - 1);
                    }
                    total += count1*count2*count3;
                }
            }
            // every triangle is counted 6 ways.
            return total / 6;
        }

        [Test]
        public void Q1Sample()
        {
            string input =
            #region SampleInput

 @"2
4 10 7 1 2 0 1 20
6 2 0 2 1 1 2 11";

            #endregion
            string expectedOutput = @"Case #1: 1
Case #2: 2";

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
            long A, B, P;
            test.Get(out A, out B, out P);
            return () => Q2Solver(A, B, P);
        }

        private long Q2Solver(long A, long B, long P)
        {
            long range = B - A + 1;
            int[] primes = MathFormulas.PrimesLessThan((int)range);
            DisjointTracker<long> sets = new DisjointTracker<long>();
            for (long i = A; i <= B; i++)
            {
                sets.Add(i);
            }
            for (int i = 0; i < primes.Length; i++)
            {
                if (primes[i] < P) continue;
                int p = primes[i];
                long first = (A + p - 1)/p*p;
                long next = first + p;
                while (next <= B)
                {
                    sets.Union(first, next);
                    next += p;
                }
            }
            long total = 0;
            for (long i = A; i <= B; i++)
            {
                if (sets.GetRepresentative(i) == i)
                    total++;
            }
            return total;
        }

        [Test]
        public void Q2Sample()
        {
            string input =
            #region SampleInput

 @"2
10 20 5
10 20 3";

            #endregion
            string expectedOutput = @"Case #1: 9
Case #2: 7";

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

        private Func<int[]> Q3Solver(GCJTestCase test)
        {
            int K;
            test.Get(out K);
            int[] line;
            test.Get(out line);
            return () => Q3Solver(K, line.Skip(1).ToArray());
        }

        private int[] Q3Solver(int K, int[] indexes)
        {
            TreeList<int> list = new TreeList<int>(Enumerable.Range(1, K));
            Dictionary<int, int> removed = new Dictionary<int, int>();
            int index = 0;
            int move = 1;
            while (list.Count > 0)
            {
                index %= list.Count;
                int origIndex = list[index];
                list.RemoveAt(index);
                removed.Add(origIndex, move);
                index += move;
                move++;
            }
            return indexes.Select(i => removed[i]).ToArray();
        }


        [Test]
        public void Q3Sample()
        {
            string input =
            #region SampleInput

 @"2
5
5 1 2 3 4 5
15
4 3 4 7 10";

            #endregion
            string expectedOutput = @"Case #1: 1 3 2 5 4
Case #2: 2 8 13 4";

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
