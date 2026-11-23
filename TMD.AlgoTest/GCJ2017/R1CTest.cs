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
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2017
{
    [TestFixture]
    public class R1CTest : BaseGCJTest
    {
        #region Question 1 - Ample Syrup

        private static Func<double> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n, out int k);
            test.GetLines(n, out long[] r, out long[] h);
            return () => Q1Solver(n, k, r, h);
        }

        private static double Q1Solver(int n, int k, long[] r, long[] h)
        {
            (long R, long Side)[] cakes = new (long R, long Side)[n];
            for (int i = 0; i < n; i++)
                cakes[i] = (r[i], 2L * r[i] * h[i]);
            Array.Sort(cakes, static (a, b) => a.R.CompareTo(b.R));

            PriorityQueue<long, long> topSides = new();
            long sideSum = 0;
            long bestOverPi = 0;

            for (int i = 0; i < n; i++)
            {
                if (i >= k - 1)
                {
                    long total = cakes[i].R * cakes[i].R + cakes[i].Side + sideSum;
                    if (total > bestOverPi)
                        bestOverPi = total;
                }

                if (k > 1)
                {
                    long s = cakes[i].Side;
                    if (topSides.Count < k - 1)
                    {
                        topSides.Enqueue(s, s);
                        sideSum += s;
                    }
                    else if (s > topSides.Peek())
                    {
                        long removed = topSides.DequeueEnqueue(s, s);
                        sideSum += s - removed;
                    }
                }
            }

            return bestOverPi * Math.PI;
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver, 1e-6);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver, 1e-6);
        }

        [Test]
        public void Q1Large()
        {
            Test(Q1Solver, 1e-6);
        }

        #endregion

        #region Question 2 - Parenting Partnering

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int ac, out int aj);
            test.GetLines(ac, out int[] cs, out int[] ce);
            test.GetLines(aj, out int[] js, out int[] je);
            return () => Q2Solver(ac, aj, cs, ce, js, je);
        }

        private static int Q2Solver(int ac, int aj, int[] cs, int[] ce, int[] js, int[] je)
        {
            int total = ac + aj;
            // Owner is the parent who MUST cover the baby during this activity:
            // 0 = Cameron covers Jamie's activity, 1 = Jamie covers Cameron's activity.
            (int Start, int End, int Owner)[] acts = new (int, int, int)[total];
            int[] remTime = [720, 720];

            for (int i = 0; i < ac; i++)
            {
                acts[i] = (cs[i], ce[i], 1);
                remTime[1] -= ce[i] - cs[i];
            }
            for (int i = 0; i < aj; i++)
            {
                acts[ac + i] = (js[i], je[i], 0);
                remTime[0] -= je[i] - js[i];
            }

            Array.Sort(acts, static (a, b) => a.Start.CompareTo(b.Start));

            int exchanges = 0;
            List<int>[] sameGaps = [[], []];

            for (int i = 0; i < total; i++)
            {
                var cur = acts[i];
                var nxt = acts[(i + 1) % total];
                int gap = nxt.Start - cur.End;
                if (i + 1 == total)
                    gap += 1440;

                if (cur.Owner != nxt.Owner)
                    exchanges++;
                else if (gap > 0)
                    sameGaps[cur.Owner].Add(gap);
            }

            for (int owner = 0; owner < 2; owner++)
            {
                sameGaps[owner].Sort();
                foreach (int gap in sameGaps[owner])
                {
                    if (remTime[owner] >= gap)
                        remTime[owner] -= gap;
                    else
                        exchanges += 2;
                }
            }

            return exchanges;
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
            Test(Q2Solver);
        }

        #endregion

        #region Question 3 - Core Training

        private static Func<double> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n, out int k);
            test.Get(out double u);
            test.Get(out double[] p);
            return () => Q3Solver(n, k, u, p);
        }

        private static double Q3Solver(int n, int k, double u, double[] p)
        {
            double[] sorted = (double[])p.Clone();
            Array.Sort(sorted);

            double[] cur = new double[n];
            double best = 0.0;

            for (int start = 0; start < n; start++)
            {
                Array.Copy(sorted, cur, n);
                double budget = u;

                // Water-fill cur[start..n-1] upwards up to level 1.0.
                int count = 1;
                int pos = start;
                while (pos + 1 < n && budget > 0.0)
                {
                    double diff = cur[pos + 1] - cur[pos];
                    if (diff * count <= budget)
                    {
                        budget -= diff * count;
                        pos++;
                        double level = cur[pos];
                        for (int j = start; j <= pos; j++)
                            cur[j] = level;
                        count++;
                    }
                    else
                    {
                        double level = cur[pos] + budget / count;
                        for (int j = start; j <= pos; j++)
                            cur[j] = level;
                        budget = 0.0;
                        break;
                    }
                }

                if (budget > 0.0)
                {
                    double level = Math.Min(1.0, cur[pos] + budget / count);
                    double used = (level - cur[pos]) * count;
                    for (int j = start; j <= pos; j++)
                        cur[j] = level;
                    budget = Math.Max(0.0, budget - used);
                }

                // Spend any leftover units boosting earlier cores up to 1.0.
                for (int j = start - 1; j >= 0 && budget > 0.0; j--)
                {
                    double add = Math.Min(budget, 1.0 - cur[j]);
                    cur[j] += add;
                    budget -= add;
                }

                double[] pmf = MathFormulas.PoissonBinomialPmf(cur);
                double prob = 0.0;
                for (int successes = k; successes <= n; successes++)
                    prob += pmf[successes];
                if (prob > best)
                    best = prob;
            }

            return best;
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
            Test(Q3Solver, 1e-6);
        }

        #endregion
    }
}
