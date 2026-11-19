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
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2010
{
    [TestFixture]
    public class QRTest : BaseGCJTest
    {
        #region Question 1 - Snapper Chain

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n, out int k);
            return () => Q1Solver(n, k);
        }

        private static string Q1Solver(int n, int k)
        {
            int mask = (1 << n) - 1;
            return (k & mask) == mask ? "ON" : "OFF";
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

        #endregion

        #region Question 2 - Fair Warning

        private static Func<BigInteger> Q2Solver(GCJTestCase test)
        {
            test.Get(out BigInteger[] values);
            return () => Q2Solver(values);
        }

        private static BigInteger Q2Solver(BigInteger[] values)
        {
            BigInteger g = BigInteger.Zero;
            for (int i = 2; i < values.Length; i++)
            {
                g = BigInteger.GreatestCommonDivisor(g, BigInteger.Abs(values[i] - values[1]));
            }

            BigInteger rem = values[1] % g;
            return rem == 0 ? BigInteger.Zero : g - rem;
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

        #region Question 3 - Theme Park

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out int r, out int k, out int n);
            test.Get(out int[] groups);
            return () => Q3Solver(r, k, n, groups);
        }

        private static long Q3Solver(int r, int k, int n, int[] groups)
        {
            int[] nextIdx = new int[n];
            long[] earned = new long[n];

            int end = 0;
            int count = 0;
            long sum = 0;
            for (int i = 0; i < n; i++)
            {
                while (count < n && sum + groups[end] <= k)
                {
                    sum += groups[end];
                    end = end + 1 == n ? 0 : end + 1;
                    count++;
                }

                nextIdx[i] = end;
                earned[i] = sum;
                sum -= groups[i];
                count--;
            }

            int[] seenRide = new int[n];
            Array.Fill(seenRide, -1);
            long[] cumEarned = new long[n];
            int cur = 0;
            long total = 0;
            bool jumped = false;

            for (int ride = 0; ride < r;)
            {
                if (!jumped && seenRide[cur] >= 0)
                {
                    int cycleLen = ride - seenRide[cur];
                    long cycleEarn = total - cumEarned[cur];
                    int skip = (r - ride) / cycleLen;
                    total += (long)skip * cycleEarn;
                    ride += skip * cycleLen;
                    jumped = true;
                    continue;
                }

                if (!jumped)
                {
                    seenRide[cur] = ride;
                    cumEarned[cur] = total;
                }

                total += earned[cur];
                cur = nextIdx[cur];
                ride++;
            }

            return total;
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
