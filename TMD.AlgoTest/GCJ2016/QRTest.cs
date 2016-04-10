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
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Algorithms.Generic;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2016
{
    [TestFixture]
    public class QRTest : BaseGCJTest
    {
        private Func<object> Q1Solver(GCJTestCase test)
        {
            int N;
            test.Get(out N);
            return () => Q1Solver(N);
        }

        private object Q1Solver(int N)
        {
            if (N == 0) return "INSOMNIA";
            HashSet<int> digits = new HashSet<int>();
            int cur = N;
            while (true)
            {
                digits.UnionWith(cur.ToDigits());
                if (digits.Count == 10) return cur;
                cur += N;
            }
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver);
        }

        [Test]
        public void Q1Large()
        {
            SlowTest(Q1Solver);
        }

        private Func<int> Q2Solver(GCJTestCase test)
        {
            string sequence;
            test.Get(out sequence);
            return () => Q2Solver(sequence);
        }

        private int Q2Solver(string sequence)
        {
            sequence += '+';
            return sequence.Zip(sequence.Skip(1), (a, b) => a != b).Count(a => a);
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver);
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

        private Func<string> Q3Solver(GCJTestCase test)
        {
            int N, J;
            test.Get(out N, out J);
            return () => Q3Solver(N, J);
        }

        private string Q3Solver(int N, int J)
        {
            Debug.Assert(N > 1);
            BigInteger cur = BigInteger.Pow(2, N-1) + 1;
            List<List<string>> results = new List<List<string>>();
            while (results.Count < J)
            {
                int[] digits = cur.ToDigits(2).ToArray();
                int[] divisors = new int[9];
                bool good = true;
                for (int i = 2; i <= 10 && good; i++)
                {
                    BigInteger value = digits.ToValueAsDigits(i);
                    divisors[i - 2] = Enumerable.Range(2, 6).FirstOrDefault(a => value%a == 0);
                    if (divisors[i - 2] == 0) good = false;
                }
                if (good)
                {
                    List<string> resultPart = new List<string>();
                    resultPart.Add(digits.ToValueAsDigits(10).ToString());
                    resultPart.AddRange(divisors.Select(a=>a.ToString()));
                    results.Add(resultPart);
                }
                cur += 2;
            }
            StringBuilder result = new StringBuilder();
            foreach (var resultPart in results)
            {
                result.AppendLine();
                result.Append(string.Join(" ", resultPart));
            }
            return result.ToString();
        }


        // The validator for these tests doesn't match the real validator. As such, changes to Q3Solver could reasonably cause failure.
        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver);
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

        private Func<object> Q4Solver(GCJTestCase test)
        {
            int K, C, S;
            test.Get(out K, out C, out S);
            return () => Q4Solver(K, C, S);
        }

        private object Q4Solver(int K, int C, int S)
        {
            if (C*S < K) return "IMPOSSIBLE";
            // Alternative implementation
            // BigInteger[] result = new BigInteger[(K+C-1)/C];
            // for (int i=0; i < K; i+= C) {
            //     int[] digits = new int[Math.Min(K-i, C)];
            //     for (int j=0; j < digits.Length; j++) {
            //         digits[j] = i+j;
            //     }
            //     result[i/C] = digits.ToValueAsDigits(K) + 1;
            // }
            // return result;
            return Enumerable.Range(0,(K+C-1)/C).Select(a => Enumerable.Range(a*C, Math.Min(K-a*C, C)).ToValueAsDigits(K) + 1).ToArray();
        }

        // The validator for these tests doesn't match the real validator. As such, changes to Q4Solver could reasonably cause failure.
        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver);
        }

        [Test]
        public void Q4Large()
        {
            SlowTest(Q4Solver);
        }
    }
}
