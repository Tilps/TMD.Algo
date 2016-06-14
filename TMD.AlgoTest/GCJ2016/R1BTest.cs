#region License

/*
Copyright (c) 2016, the TMD.Algo authors.
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
    public class R1BTest : BaseGCJTest
    {
        private Func<string> Q1Solver(GCJTestCase test)
        {
            string line;
            test.Get(out line);
            return () => Q1Solver(line);
        }

        private string Q1Solver(string line)
        {
            Dictionary<char, int> freqs = line.GroupBy(c => c).ToDictionary(a => a.Key, a => a.Count());
            string[] checkOrder = new[] {"ZERO", "TWO", "FOUR", "SIX", "EIGHT", "ONE", "THREE", "FIVE", "SEVEN", "NINE"};
            int[] valueOrder = new[] {0, 2, 4, 6, 8, 1, 3, 5, 7, 9};
            int[] output = checkOrder.Select(s =>
            {
                int count = s.Min(c => freqs.GetOrDefault(c, 0));
                foreach (char c in s) freqs.UpdateValueForKey(c, x => x - count);
                return count;
            }).ToArray();
            Array.Sort(valueOrder, output);
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < output[i]; j++)
                {
                    result.Append(i.ToString());
                }
            }
            return result.ToString();
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

        private Func<string> Q2Solver(GCJTestCase test)
        {
            string[] scores;
            test.Get(out scores);
            return () => Q2Solver(scores[0], scores[1]);
        }

        private string Q2Solver(string C, string J)
        {
            BigInteger bestDiff = long.MaxValue;
            BigInteger bestC = 0;
            BigInteger bestJ = 0;
            for (int i = 0; i <= C.Length; i++)
            {
                for (int k = 0; k < 2; k++)
                {
                    StringBuilder fillInC = new StringBuilder();
                    StringBuilder fillInJ = new StringBuilder();
                    for (int j = 0; j < C.Length; j++)
                    {
                        if (C[j] != '?') fillInC.Append(C[j]);
                        if (J[j] != '?') fillInJ.Append(J[j]);
                        if (C[j] != '?' && J[j] != '?') continue;
                        if (j < i)
                        {
                            if (C[j] == J[j])
                            {
                                fillInC.Append('0');
                                fillInJ.Append('0');
                            }
                            else if (C[j] == '?')
                            {
                                fillInC.Append(J[j]);
                            }
                            else
                            {
                                fillInJ.Append(C[j]);
                            }
                        }
                        else if (j == i)
                        {
                            if (k == 0)
                            {
                                if (C[j] == J[j])
                                {
                                    fillInC.Append('0');
                                    fillInJ.Append('1');
                                }
                                else if (C[j] == '?')
                                {
                                    int digit = int.Parse("" + J[j]);
                                    fillInC.Append(Math.Max(0, digit - 1).ToString());
                                }
                                else
                                {
                                    int digit = int.Parse("" + C[j]);
                                    fillInJ.Append(Math.Min(9, digit + 1).ToString());
                                }
                            }
                            else
                            {
                                if (C[j] == J[j])
                                {
                                    fillInC.Append('1');
                                    fillInJ.Append('0');
                                }
                                else if (C[j] == '?')
                                {
                                    int digit = int.Parse("" + J[j]);
                                    fillInC.Append(Math.Min(9, digit + 1).ToString());
                                }
                                else
                                {
                                    int digit = int.Parse("" + C[j]);
                                    fillInJ.Append(Math.Max(0, digit - 1).ToString());
                                }
                            }
                        }
                        else
                        {
                            if (k == 0)
                            {
                                if (C[j] == '?')
                                {
                                    fillInC.Append('9');
                                }
                                if (J[j] == '?')
                                {
                                    fillInJ.Append('0');
                                }
                            }
                            else
                            {
                                if (C[j] == '?')
                                {
                                    fillInC.Append('0');
                                }
                                if (J[j] == '?')
                                {
                                    fillInJ.Append('9');
                                }
                            }
                        }
                    }
                    BigInteger cTrial =
                        fillInC.ToString().Select(a => int.Parse("" + a)).Reverse().ToValueAsDigits();
                    BigInteger jTrial =
                        fillInJ.ToString().Select(a => int.Parse("" + a)).Reverse().ToValueAsDigits();
                    BigInteger diff = cTrial - jTrial;
                    if (diff < 0) diff = -diff;
                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestC = cTrial;
                        bestJ = jTrial;
                    }
                    else if (diff == bestDiff)
                    {
                        if (cTrial < bestC)
                        {
                            bestC = cTrial;
                            bestJ = jTrial;
                        }
                        else if (cTrial == bestC)
                        {
                            if (jTrial < bestJ)
                            {
                                bestJ = jTrial;
                            }
                        }
                    }
                }
            }
            return string.Format("{0} {1}", bestC.ToString(new string('0', C.Length)),
                bestJ.ToString(new string('0', J.Length)));
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

        private Func<int> Q3Solver(GCJTestCase test)
        {
            int A;
            test.Get(out A);
            string[][] data = new string[A][];
            for (int i = 0; i < A; i++)
            {
                string[] parts;
                test.Get(out parts);
                data[i] = parts;
            }
            return () => Q3Solver(data);
        }

        private int Q3Solver(string[][] data)
        {
            Graph<int, int> g = new Graph<int, int>();
            Dictionary<string, int> firstNodes = new Dictionary<string, int>();
            Dictionary<string, int> secondNodes = new Dictionary<string, int>();
            foreach (string[] pair in data)
            {
                int firstNode = firstNodes.GetOrPopulate(pair[0], () => g.AddVertex(0));
                int secondNode = secondNodes.GetOrPopulate(pair[1], () => g.AddVertex(0));
                g.AddUndirectedEdge(firstNode, secondNode, 0);
            }
            int j = g.MaximumBipartiteIndependentSet();
            return data.Length - j;
        }


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
    }
}
