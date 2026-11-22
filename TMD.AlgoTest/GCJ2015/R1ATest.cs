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
using System.Globalization;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2015
{
    [TestFixture]
    public class R1ATest : BaseGCJTest
    {
        #region Question 1 - Mushroom Monster

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int _);
            test.Get(out int[] m);
            return () => Q1Solver(m);
        }

        private static string Q1Solver(int[] m)
        {
            int m1 = 0;
            int maxRate = 0;
            for (int i = 1; i < m.Length; i++)
            {
                int drop = m[i - 1] - m[i];
                if (drop > 0)
                {
                    m1 += drop;
                    if (drop > maxRate)
                        maxRate = drop;
                }
            }

            int m2 = 0;
            for (int i = 0; i < m.Length - 1; i++)
                m2 += Math.Min(m[i], maxRate);

            return $"{m1} {m2}";
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

        #endregion

        #region Question 2 - Haircut

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int _, out long n);
            test.Get(out int[] m);
            return () => Q2Solver(n, m);
        }

        private static int Q2Solver(long n, int[] m)
        {
            long ServedBy(long time)
            {
                if (time < 0)
                    return 0;
                long total = 0;
                foreach (int mk in m)
                    total += time / mk + 1;
                return total;
            }

            int minM = int.MaxValue;
            foreach (int mk in m)
            {
                if (mk < minM)
                    minM = mk;
            }

            long high = (long)minM * (n - 1);
            long transition = SpecialtySelect.FindTransition(-1L, high, t => ServedBy(t) >= n);
            long targetTime = transition + 1;

            long rem = n - ServedBy(targetTime - 1);
            for (int i = 0; i < m.Length; i++)
            {
                if (targetTime % m[i] == 0)
                {
                    rem--;
                    if (rem == 0)
                        return i + 1;
                }
            }
            throw new InvalidOperationException("Unreachable barber assignment state.");
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

        #region Question 3 - Logging

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out long[] x, out long[] y);
            return () => Q3Solver(n, x, y);
        }

        private static string Q3Solver(int n, long[] x, long[] y)
        {
            StringBuilder sb = new();
            if (n <= 3)
            {
                for (int i = 0; i < n; i++)
                    sb.Append('\n').Append('0');
                return sb.ToString();
            }

            int m = n - 1;
            Point2L[] pts = new Point2L[n];
            for (int i = 0; i < n; i++)
                pts[i] = new Point2L(x[i], y[i]);

            Point2L[] rel = new Point2L[m];
            PolarAngleComparer cmp = new(default);

            for (int i = 0; i < n; i++)
            {
                Point2L origin = pts[i];
                int idx = 0;
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                        rel[idx++] = pts[j] - origin;
                }

                rel.AsSpan().Sort(cmp);

                int head = 0;
                int tail = 0;
                int minCut = m;
                while (tail < m)
                {
                    Point2L pTail = rel[tail];
                    int tailEnd = tail + 1;
                    while (tailEnd < m && Geometry.CompareDirectionCCW(pTail, rel[tailEnd]) == 0)
                        tailEnd++;

                    if (head < tailEnd)
                        head = tailEnd;

                    while (head < tail + m)
                    {
                        Point2L pHead = rel[head % m];
                        long cross = pTail.X * pHead.Y - pTail.Y * pHead.X;
                        if (cross <= 0)
                            break;
                        head++;
                    }

                    int cut = head - tailEnd;
                    if (cut < minCut)
                    {
                        minCut = cut;
                        if (minCut == 0)
                            break;
                    }

                    tail = tailEnd;
                }

                sb.Append('\n').Append(minCut.ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString();
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

        #endregion
    }
}
