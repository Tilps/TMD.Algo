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
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2015
{
    [TestFixture]
    public class R1BTest : BaseGCJTest
    {
        #region Question 1 - Counter Culture

        private static readonly long[] Pow10 = BuildPow10();

        private static long[] BuildPow10()
        {
            long[] p = new long[16];
            p[0] = 1;
            for (int i = 1; i < p.Length; i++)
                p[i] = p[i - 1] * 10;
            return p;
        }

        private static Func<long> Q1Solver(GCJTestCase test)
        {
            test.Get(out long n);
            return () => Q1Solver(n);
        }

        private static long Q1Solver(long n)
        {
            if (n < 10)
                return n;

            string s = n.ToString(CultureInfo.InvariantCulture);
            int m = s.Length;
            int half = m / 2;
            string left = s[..half];
            string right = s[half..];
            long rVal = long.Parse(right, CultureInfo.InvariantCulture);
            if (rVal == 0)
                return Q1Solver(n - 1) + 1;

            long ans = 1;
            for (int d = 1; d < m; d++)
            {
                ans += Pow10[(d + 1) / 2] - 1;
                if (d > 1)
                    ans++;
                ans += Pow10[d / 2] - 1;
            }

            long lRev = 0;
            for (int i = left.Length - 1; i >= 0; i--)
                lRev = lRev * 10 + (left[i] - '0');

            if (lRev == 1)
                return ans + rVal;

            return ans + lRev + rVal;
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

        #region Question 2 - Noisy Neighbors

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c, out int n);
            return () => Q2Solver(r, c, n);
        }

        private static int Q2Solver(int r, int c, int n)
        {
            if (r > c)
                (r, c) = (c, r);

            if (n <= (r * c + 1) / 2)
                return 0;

            int kRemove = r * c - n;
            if (r == 1)
                return (c - 1) - 2 * kRemove;

            int Score(int all, int corners, int inners)
            {
                int k = kRemove;
                int sides = all - corners - inners;
                int unhappiness = 2 * r * c - r - c;

                int take4 = Math.Min(k, inners);
                k -= take4;
                unhappiness -= take4 * 4;

                int take3 = Math.Min(k, sides);
                k -= take3;
                unhappiness -= take3 * 3;

                int take2 = Math.Min(k, corners);
                unhappiness -= take2 * 2;
                return unhappiness;
            }

            if ((r & 1) == 1 && (c & 1) == 1)
            {
                int p1 = Score((r * c + 1) / 2, 4, ((r - 2) * (c - 2) + 1) / 2);
                int p2 = Score((r * c) / 2, 0, ((r - 2) * (c - 2)) / 2);
                return Math.Min(p1, p2);
            }

            return Score((r * c) / 2, 2, ((r - 2) * (c - 2)) / 2);
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

        #region Question 3 - Hiking Deer

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out int[] d, out int[] h, out int[] m);
            return () => Q3Solver(d, h, m);
        }

        private static int Q3Solver(int[] d, int[] h, int[] m)
        {
            int totalHikers = 0;
            foreach (int hi in h)
                totalHikers += hi;

            PriorityQueue<(long Step, bool IsFirst), long> pq = new(totalHikers);
            for (int g = 0; g < d.Length; g++)
            {
                long remDeg = 360 - d[g];
                for (int k = 0; k < h[g]; k++)
                {
                    long mk = m[g] + (long)k;
                    pq.Enqueue((360L * mk, true), remDeg * mk);
                }
            }

            int current = totalHikers;
            int best = totalHikers;
            int remainingFirst = totalHikers;

            while (pq.Count > 0 && current - remainingFirst < best)
            {
                pq.TryPeek(out _, out long curTime);
                while (pq.TryPeek(out _, out long nextTime) && nextTime == curTime)
                {
                    (long step, bool isFirst) = pq.Dequeue();
                    if (isFirst)
                    {
                        current--;
                        remainingFirst--;
                    }
                    else
                    {
                        current++;
                    }
                    pq.Enqueue((step, false), curTime + step);
                }
                if (current < best)
                    best = current;
            }
            return best;
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Small1()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Small2()
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
