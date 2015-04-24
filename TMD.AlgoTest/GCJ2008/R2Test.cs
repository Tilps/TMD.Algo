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
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2008
{
    [TestFixture]
    public class R2Test : BaseGCJTest
    {
        private Func<object> Q1Solver(GCJTestCase test)
        {
            int M, V;
            test.Get(out M, out V);
            int[] Gs;
            int[] Cs;
            test.GetLines((M-1)/2, out Gs, out Cs);
            int[] Is;
            test.GetLines((M+1)/2, out Is);
            return () => Q1Solver(M, V, Gs, Cs, Is);
        }

        private object Q1Solver(int M, int V, int[] Gs, int[] Cs, int[] Is)
        {
            int terminalCost = 2*M;
            int[,] minCost = new int[M, 2];
            for (int i = M - 1; i >= 0; i--)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (i >= Gs.Length)
                    {
                        minCost[i, j] = Is[i - Gs.Length] == j ? 0 : terminalCost;
                    }
                    else
                    {
                        int child1 = (i + 1)*2 - 1;
                        int child2 = (i + 1)*2;
                        List<int> andBests = new List<int>();
                        List<int> orBests = new List<int>();
                        for (int k = 0; k < 2; k++)
                        {
                            for (int l = 0; l < 2; l++)
                            {
                                if ((k & l) == j)
                                {
                                    andBests.Add(minCost[child1, k] + minCost[child2, l]);
                                }
                                if ((k | l) == j)
                                {
                                    orBests.Add(minCost[child1, k] + minCost[child2, l]);
                                }
                            }
                        }
                        int[] bests = {orBests.Min(), andBests.Min()};
                        minCost[i, j] = Math.Min(bests[Gs[i]], Cs[i] == 1 ? bests[(Gs[i] + 1)%2] + 1 : terminalCost);
                    }
                }
            }
            int result = minCost[0, V];
            if (result > M) return "IMPOSSIBLE";
            return result;
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
            Test(Q1Solver);
        }

        private Func<object> Q2Solver(GCJTestCase test)
        {
            int N, M, A;
            test.Get(out N, out M, out A);
            return () => Q2Solver(N, M, A);
        }

        private object Q2Solver(int N, int M, int A)
        {
            if (N*M < A) return "IMPOSSIBLE";
            int[] point1 = {1, 0};
            int[] point2 = {0, M};
            int x = (A - 1)/M + 1;
            int y = (A - 1)%M + 1;
            int[] point3 = {x, y};
            return new int[][]
            {
                point1,
                point2,
                point3
            };
        }

        // The validator for these tests does not match real validator, as such failures are expected if implementation changes.
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

        private Func<object> Q3Solver(GCJTestCase test)
        {
            int N;
            test.Get(out N);
            int[][] ships;
            test.GetMatrix(N, out ships);
            return () => Q3Solver(ships);
        }

        private object Q3Solver(int[][] ships)
        {
            Func<double, bool> canDo = power =>
            {
                double[,] ranges = new double[4, 2];
                for (int i = 0; i < 4; i++)
                {
                    ranges[i, 0] = double.MinValue;
                    ranges[i, 1] = double.MaxValue;
                }
                for (int i = 0; i < ships.Length; i++)
                {
                    double[,] shipRange = new double[4, 2];
                    shipRange[0, 0] = ships[i][0] + ships[i][1] + ships[i][2] - ships[i][3]*power;
                    shipRange[0, 1] = ships[i][0] + ships[i][1] + ships[i][2] + ships[i][3]*power;
                    shipRange[1, 0] = ships[i][0] + ships[i][1] - ships[i][2] - ships[i][3]*power;
                    shipRange[1, 1] = ships[i][0] + ships[i][1] - ships[i][2] + ships[i][3]*power;
                    shipRange[2, 0] = ships[i][0] - ships[i][1] + ships[i][2] - ships[i][3]*power;
                    shipRange[2, 1] = ships[i][0] - ships[i][1] + ships[i][2] + ships[i][3]*power;
                    shipRange[3, 0] = -ships[i][0] + ships[i][1] + ships[i][2] - ships[i][3]*power;
                    shipRange[3, 1] = -ships[i][0] + ships[i][1] + ships[i][2] + ships[i][3]*power;
                    for (int j = 0; j < 4; j++)
                    {
                        ranges[j, 0] = Math.Max(shipRange[j, 0], ranges[j, 0]);
                        ranges[j, 1] = Math.Min(shipRange[j, 1], ranges[j, 1]);
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    if (ranges[i, 0] > ranges[i, 1]) return false;
                }
                // 4 slabs are non-empty, but that is not enough to determine intersection non-empty.
                double xMin1 = (ranges[0, 0] - ranges[3, 1])/2;
                double xMax1 = (ranges[0, 1] - ranges[3, 0])/2;
                double xMin2 = (ranges[1, 0] + ranges[2, 0])/2;
                double xMax2 = (ranges[1, 1] + ranges[2, 1])/2;
                return Math.Max(xMin1, xMin2) <= Math.Min(xMax1, xMax2);
            };
            if (!canDo(1e8))
            {
                return "IMPOSSIBLE";
            }
            if (canDo(0))
            {
                return 0.0;
            }
            return SpecialtySelect.FindTransition(0, 1e8, canDo) + double.Epsilon;
        }


        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, 1e-6);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, 1e-6);
        }

        [Test]
        public void Q3Large()
        {
            SlowTest(Q3Solver, 1e-6);
        }

        private Func<int> Q4Solver(GCJTestCase test)
        {
            int k;
            test.Get(out k);
            string word;
            test.Get(out word);
            return () => Q4Solver(k, word);
        }

        private int Q4Solver(int k, string word)
        {
            int best = word.Length;
            for (int i = 0; i < k; i++)
            {
                int[,] weights = new int[k, k];
                for (int j = 0; j < k; j++)
                {
                    for (int l = 0; l < k; l++)
                    {
                        if (j == l) continue;
                        for (int m = 0; m < word.Length/k; m++)
                        {
                            if (j != i)
                            {
                                if (word[m*k +j] != word[m*k+l])
                                    weights[j, l]++;
                            }
                            else if (m != 0)
                            {
                                if (word[(m-1)*k+j] != word[m*k+l])
                                    weights[j, l]++;
                            }
                        }
                    }
                }
                // TODO: extract this to the graph class.
                int[,] dp = new int[1<<k,k];
                for (int j = 1; j < 1 << k; j++)
                {
                    for (int l = 0; l < k; l++)
                    {
                        if ((j & (1 << l)) == 0) continue;
                        dp[j, l] = int.MaxValue;
                        int child = j - (1 << l);
                        if (child == 0)
                        {
                            dp[j, l] = weights[l, 0];
                        }
                        else
                        {
                            for (int m = 0; m < k; m++)
                            {
                                if ((child & (1 << m)) == 0) continue;
                                dp[j, l] = Math.Min(dp[j, l], dp[child, m] + weights[l,m]);
                            }
                        }
                    }
                }
                int candidate = dp[(1 << k) - 1, 0];
                if (candidate < best) best = candidate;
            }
            return best + 1;
        }


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
