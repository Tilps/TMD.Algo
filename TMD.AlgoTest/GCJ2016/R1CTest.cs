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
    public class R1CTest : BaseGCJTest
    {
        private Func<string> Q1Solver(GCJTestCase test)
        {
            int A;
            test.Get(out A);
            int[] counts;
            test.Get(out counts);
            return () => Q1Solver(counts);
        }

        private string Q1Solver(int[] counts)
        {
            StringBuilder result = new StringBuilder();
            while (true)
            {
                int total = counts.Count(a=>a>0);
                if (total == 0) break;
                if (result.Length > 0) result.Append(" ");
                if (total <= 2)
                {
                    for (int i = 0; i < counts.Length; i++)
                    {
                        if (counts[i] > 0)
                        {
                            result.Append((char) ('A' + i));
                            counts[i]--;
                        }
                    }
                }
                else
                {
                    int maxIndex = -1;
                    int maxValue = int.MinValue;
                    for (int i = 0; i < counts.Length; i++)
                    {
                        if (counts[i] > 0) total++;
                        if (counts[i] > maxValue)
                        {
                            maxIndex = i;
                            maxValue = counts[i];
                        }
                    }
                    result.Append((char) ('A' + maxIndex));
                    counts[maxIndex]--;
                }
            }
            return result.ToString();
        }

        // This problem has multiple solutions, but validation is only by exact match.
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
            long B, M;
            test.Get(out B, out M);
            return () => Q2Solver(B, M);
        }

        private string Q2Solver(long B, long M)
        {
            long[] max = new long[B];
            max[0] = 1;
            for (int i = 1; i < B; i++)
            {
                max[i] = max.Take(i).Sum();
            }
            if (M > max[B - 1]) return "IMPOSSIBLE";
            StringBuilder result = new StringBuilder();
            result.AppendLine("POSSIBLE");
            bool[] needed = new bool[B - 1];
            for (long i = B - 2; i >= 0; i--)
            {
                if (M >= max[i])
                {
                    needed[i] = true;
                    M -= max[i];
                }
            }
            for (int i = 0; i < B - 1; i++)
            {
                string basis = new string('0', i + 1);
                if (i < B - 2)
                {
                    basis += new string('1', (int)B - 2 - i);
                }
                if (needed[i])
                {
                    result.AppendLine(basis + '1');
                }
                else
                {
                    result.AppendLine(basis + '0');
                }
            }
            result.Append(new string('0', (int)B));
            return result.ToString();
        }

        // This problem has multiple solutions, but validation is only by exact match.
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
            int J, P, S, K;
            test.Get(out J, out P, out S, out K);
            return () => Q3Solver(J, P, S, K);
        }

        private string Q3Solver(int J, int P, int S, int K)
        {
            int[,] firstCounts = new int[12,12];
            int[,] secondCounts = new int[12,12];
            int[,] thirdCounts = new int[12,12];
            List<int[]> ans = new List<int[]>();

            for (int i = 1; i <= J; i++)
            {
                for (int j = 1; j <= P; j++)
                {
                    if (firstCounts[i, j] >= K) continue;
                    int first = i + j - 2;
                    for (int M = 0; M < S; M++)
                    {
                        int k = (first + M) % S + 1;
                        if (firstCounts[i, j] >= K) continue;
                        if (secondCounts[i, k] >= K) continue;
                        if (thirdCounts[j, k] >= K) continue;
                        firstCounts[i, j]++;
                        secondCounts[i, k]++;
                        thirdCounts[j, k]++;
                        ans.Add(new int[] { i, j, k });
                    }
                }
            }
            return BuildResult(ans);
        }

        private static string BuildResult(List<int[]> ans)
        {
            StringBuilder result = new StringBuilder();
            result.Append(ans.Count.ToString());
            foreach (int[] a in ans)
            {
                result.AppendLine();
                result.Append(string.Join(" ", a));
            }
            return result.ToString();
        }


        // This problem has multiple solutions, but validation is only by exact match.
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
