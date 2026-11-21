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
using System.Globalization;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2012
{
    [TestFixture]
    public class R1ATest : BaseGCJTest
    {
        #region Question 1 - Password Problem

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int a, out int b);
            test.Get(out double[] p);
            return () => Q1Solver(a, b, p);
        }

        private static string Q1Solver(int a, int b, double[] p)
        {
            double best = b + 2.0;
            double prefProd = 1.0;
            for (int m = 1; m <= a; m++)
            {
                prefProd *= p[m - 1];
                double expected = (a - m) + (b - m) + 1 + (b + 1) * (1.0 - prefProd);
                if (expected < best)
                    best = expected;
            }
            return best.ToString("F6", CultureInfo.InvariantCulture);
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

        #region Question 2 - Kingdom Rush

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out (int A, int B)[] levels);
            return () => Q2Solver(levels);
        }

        private static string Q2Solver((int A, int B)[] levels)
        {
            int n = levels.Length;
            int[] earned = new int[n];
            int stars = 0;
            int completedTwoStar = 0;
            int plays = 0;

            while (completedTwoStar < n)
            {
                bool didTwoStar = false;
                for (int i = 0; i < n; i++)
                {
                    if (earned[i] < 2 && levels[i].B <= stars)
                    {
                        stars += 2 - earned[i];
                        earned[i] = 2;
                        completedTwoStar++;
                        plays++;
                        didTwoStar = true;
                        break;
                    }
                }
                if (didTwoStar)
                    continue;

                int bestOneStar = -1;
                for (int i = 0; i < n; i++)
                {
                    if (earned[i] == 0 && levels[i].A <= stars)
                    {
                        if (bestOneStar < 0 || levels[i].B > levels[bestOneStar].B)
                            bestOneStar = i;
                    }
                }

                if (bestOneStar < 0)
                    return "Too Bad";

                earned[bestOneStar] = 1;
                stars++;
                plays++;
            }

            return plays.ToString(CultureInfo.InvariantCulture);
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

        #region Question 3 - Cruise Control

        private readonly record struct CruiseEvent(long Num, long Den, bool IsStart, int I, int J);

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out string[] laneTokens, out int[] s, out int[] p);
            char[] lane = new char[n];
            for (int i = 0; i < n; i++)
                lane[i] = laneTokens[i][0];
            return () => Q3Solver(lane, s, p);
        }

        private static string Q3Solver(char[] lane, int[] s, int[] p)
        {
            int n = lane.Length;
            ParityDisjointTracker dsu = new(2);
            dsu.UnionOpposite(0, 1);
            int[] node = new int[n];
            Array.Fill(node, -1);
            int[] deg = new int[n];
            List<CruiseEvent> events = [];

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    int slow = s[i] <= s[j] ? i : j;
                    int fast = s[i] <= s[j] ? j : i;
                    long ds = s[fast] - s[slow];

                    if (Math.Abs(p[i] - p[j]) < 5)
                    {
                        if (node[i] < 0) node[i] = lane[i] == 'L' ? 0 : 1;
                        if (node[j] < 0) node[j] = lane[j] == 'L' ? 0 : 1;
                        deg[i]++;
                        deg[j]++;
                        if (!dsu.UnionOpposite(node[i], node[j]))
                            return "0.000000";

                        if (ds > 0)
                            events.Add(new CruiseEvent((long)(p[slow] - p[fast]) + 5, ds, false, i, j));
                    }
                    else if (ds > 0 && p[slow] - p[fast] >= 5)
                    {
                        long numStart = (long)(p[slow] - p[fast]) - 5;
                        long numEnd = (long)(p[slow] - p[fast]) + 5;
                        events.Add(new CruiseEvent(numStart, ds, true, i, j));
                        events.Add(new CruiseEvent(numEnd, ds, false, i, j));
                    }
                }
            }

            events.Sort(static (a, b) =>
            {
                int cmp = (a.Num * b.Den).CompareTo(b.Num * a.Den);
                if (cmp != 0)
                    return cmp;
                return a.IsStart.CompareTo(b.IsStart); // Process END events before START events at exact ties.
            });

            foreach (var ev in events)
            {
                if (!ev.IsStart)
                {
                    deg[ev.I]--;
                    if (deg[ev.I] == 0)
                        node[ev.I] = -1;
                    deg[ev.J]--;
                    if (deg[ev.J] == 0)
                        node[ev.J] = -1;
                }
                else
                {
                    if (node[ev.I] < 0)
                        node[ev.I] = dsu.Add();
                    if (node[ev.J] < 0)
                        node[ev.J] = dsu.Add();
                    deg[ev.I]++;
                    deg[ev.J]++;
                    if (!dsu.UnionOpposite(node[ev.I], node[ev.J]))
                        return ((double)ev.Num / ev.Den).ToString("F6", CultureInfo.InvariantCulture);
                }
            }

            return "Possible";
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, 1e-5);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, 1e-5);
        }

        [Test]
        public void Q3Large()
        {
            Test(Q3Solver, 1e-5);
        }

        #endregion
    }
}
