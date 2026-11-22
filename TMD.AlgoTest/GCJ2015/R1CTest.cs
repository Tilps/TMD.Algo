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
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2015
{
    [TestFixture]
    public class R1CTest : BaseGCJTest
    {
        #region Question 1 - Brattleship

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c, out int w);
            return () => Q1Solver(r, c, w);
        }

        private static int Q1Solver(int r, int c, int w)
        {
            return r * (c / w) + (w - 1) + (c % w != 0 ? 1 : 0);
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

        #region Question 2 - Typewriter Monkey

        private static Func<double> Q2Solver(GCJTestCase test)
        {
            test.Get(out int k, out int l, out int s);
            test.Get(out string keyboard);
            test.Get(out string target);
            return () => Q2Solver(k, l, s, keyboard, target);
        }

        private static double Q2Solver(int k, int l, int s, string keyboard, string target)
        {
            int[] counts = new int[26];
            foreach (char ch in keyboard)
                counts[ch - 'A']++;

            double prob = 1.0;
            foreach (char ch in target)
            {
                int c = counts[ch - 'A'];
                if (c == 0)
                    return 0.0;
                prob *= (double)c / k;
            }

            int[] pi = Pattern.ComputePrefixFunction(target.AsSpan());
            int overlap = pi[l - 1];
            int maxCopies = 1 + (s - l) / (l - overlap);
            double expectedCopies = prob * (s - l + 1);
            return maxCopies - expectedCopies;
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, 1e-6);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver, 1e-6);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver, 1e-6);
        }

        #endregion

        #region Question 3 - Less Money, More Problems

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out long c, out int _, out long v);
            test.Get(out long[] coins);
            return () => Q3Solver(c, v, coins);
        }

        private static int Q3Solver(long c, long v, long[] coins)
        {
            long reach = 0;
            int idx = 0;
            int added = 0;

            while (reach < v)
            {
                if (idx < coins.Length && coins[idx] <= reach + 1)
                {
                    reach += coins[idx++] * c;
                }
                else
                {
                    added++;
                    reach += (reach + 1) * c;
                }
            }

            return added;
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
            Test(Q3Solver);
        }

        #endregion
    }
}
