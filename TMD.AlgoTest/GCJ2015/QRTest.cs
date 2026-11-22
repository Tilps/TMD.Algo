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
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2015
{
    [TestFixture]
    public class QRTest : BaseGCJTest
    {
        #region Question 1 - Standing Ovation

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int sMax, out string s);
            return () => Q1Solver(sMax, s);
        }

        private static int Q1Solver(int sMax, string s)
        {
            int standing = 0;
            int friends = 0;
            for (int k = 0; k <= sMax; k++)
            {
                int count = s[k] - '0';
                if (count > 0 && standing < k)
                {
                    friends += k - standing;
                    standing = k;
                }
                standing += count;
            }
            return friends;
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

        #region Question 2 - Infinite House of Pancakes

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int _);
            test.Get(out int[] p);
            return () => Q2Solver(p);
        }

        private static int Q2Solver(int[] p)
        {
            int maxP = 0;
            foreach (int v in p)
            {
                if (v > maxP) maxP = v;
            }

            int best = maxP;
            for (int x = 1; x < best; x++)
            {
                int moves = 0;
                foreach (int pi in p)
                    moves += (pi - 1) / x;
                if (x + moves < best)
                    best = x + moves;
            }
            return best;
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

        #region Question 3 - Dijkstra

        private static readonly int[][] QuatTable =
        [
            [0, 0, 0, 0, 0],
            [0, 1, 2, 3, 4],
            [0, 2, -1, 4, -3],
            [0, 3, -4, -1, 2],
            [0, 4, 3, -2, -1]
        ];

        private static int QuatMul(int a, int b)
        {
            int prod = QuatTable[Math.Abs(a)][Math.Abs(b)];
            return (a ^ b) < 0 ? -prod : prod;
        }

        private static int QuatPow(int a, long exp)
        {
            int res = 1;
            int count = (int)(exp & 3);
            for (int i = 0; i < count; i++)
                res = QuatMul(res, a);
            return res;
        }

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int l, out long x);
            test.Get(out string s);
            return () => Q3Solver(l, x, s);
        }

        private static string Q3Solver(int l, long x, string s)
        {
            int[] q = new int[l];
            int blockProd = 1;
            for (int i = 0; i < l; i++)
            {
                int val = s[i] - 'i' + 2;
                q[i] = val;
                blockProd = QuatMul(blockProd, val);
            }

            if (QuatPow(blockProd, x) != -1)
                return "NO";

            int copies = (int)Math.Min(x, 8L);
            int cur = 1;
            bool gotI = false;
            bool gotJ = false;
            for (int c = 0; c < copies && !gotJ; c++)
            {
                for (int i = 0; i < l; i++)
                {
                    cur = QuatMul(cur, q[i]);
                    if (!gotI)
                    {
                        if (cur == 2)
                        {
                            gotI = true;
                            cur = 1;
                        }
                    }
                    else if (cur == 3)
                    {
                        gotJ = true;
                        break;
                    }
                }
            }

            return gotI && gotJ ? "YES" : "NO";
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

        #region Question 4 - Ominous Omino

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int x, out int r, out int c);
            return () => Q4Solver(x, r, c);
        }

        private static string Q4Solver(int x, int r, int c)
        {
            int s = Math.Min(r, c);
            int l = Math.Max(r, c);
            bool richardWins =
                (r * c) % x != 0 ||
                x >= 7 ||
                (x == 3 && s == 1) ||
                (x == 4 && s <= 2) ||
                (x == 5 && (s <= 2 || (s == 3 && l == 5))) ||
                (x == 6 && s <= 3);
            return richardWins ? "RICHARD" : "GABRIEL";
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
            Test(Q4Solver);
        }

        #endregion
    }
}
