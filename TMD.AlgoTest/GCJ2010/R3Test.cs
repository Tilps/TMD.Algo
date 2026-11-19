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
using System.Linq;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2010
{
    [TestFixture]
    public class R3Test : BaseGCJTest
    {
        #region Question 1 - De-RNG-ed

        private static readonly int[] Primes = MathFormulas.PrimesLessThan(1_000_000);
        private static readonly int[] Pow10 = [1, 10, 100, 1_000, 10_000, 100_000, 1_000_000];

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int d, out int k);
            test.Get(out int[] seq);
            return () => Q1Solver(d, k, seq);
        }

        private static string Q1Solver(int d, int k, int[] seq)
        {
            if (k <= 1)
                return "I don't know.";
            if (seq[0] == seq[1])
                return seq[0].ToString();
            if (k == 2)
                return "I don't know.";

            int maxVal = 0;
            for (int i = 0; i < seq.Length; i++)
            {
                if (seq[i] > maxVal)
                    maxVal = seq[i];
            }

            int maxPrime = Pow10[d];
            int startIdx = Array.BinarySearch(Primes, maxVal + 1);
            if (startIdx < 0)
                startIdx = ~startIdx;

            long d0 = seq[1] - seq[0];
            long d1 = seq[2] - seq[1];
            int candidate = -1;

            for (int idx = startIdx; idx < Primes.Length; idx++)
            {
                int p = Primes[idx];
                if (p > maxPrime)
                    break;

                long diff0 = ((d0 % p) + p) % p;
                long diff1 = ((d1 % p) + p) % p;
                long a = (diff1 * Modulo.Invert(diff0, p)) % p;
                long b = (((seq[1] - a * seq[0]) % p) + p) % p;

                bool ok = true;
                for (int i = 3; i < k; i++)
                {
                    if ((a * seq[i - 1] + b) % p != seq[i])
                    {
                        ok = false;
                        break;
                    }
                }
                if (!ok)
                    continue;

                int next = (int)((a * seq[k - 1] + b) % p);
                if (candidate < 0)
                {
                    candidate = next;
                }
                else if (candidate != next)
                {
                    return "I don't know.";
                }
            }

            return candidate.ToString();
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

        #region Question 2 - Fence

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out long l, out int _);
            test.Get(out int[] boards);
            return () => Q2Solver(l, boards);
        }

        private static string Q2Solver(long l, int[] boards)
        {
            int[] unique = boards.Distinct().ToArray();
            int maxB = unique.Max();
            long p = l / maxB;
            int q = (int)(l % maxB);
            int[] others = unique.Where(b => b != maxB).ToArray();

            int dist = Search.ZeroOnePfs(maxB, 0, q, r =>
            {
                (int, int)[] edges = new (int, int)[others.Length];
                for (int i = 0; i < others.Length; i++)
                {
                    int next = r + others[i];
                    if (next >= maxB)
                        edges[i] = (next - maxB, 0);
                    else
                        edges[i] = (next, 1);
                }
                return edges;
            });

            return dist < 0 ? "IMPOSSIBLE" : (p + dist).ToString();
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

        #endregion

        #region Question 3 - Hot Dog Proliferation

        private static Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out int c);
            test.GetLines(c, out (long Pos, int Count)[] piles);
            return () => Q3Solver(piles);
        }

        private static long Q3Solver((long Pos, int Count)[] piles)
        {
            Stack<(long L, long R)> leftStack = new();
            Stack<(long L, long R)> rightStack = new();
            long moves = 0;

            void PushLeftMerged(long l, long r)
            {
                if (leftStack.Count > 0 && leftStack.Peek().R + 1 == l)
                    l = leftStack.Pop().L;
                leftStack.Push((l, r));
            }

            void PushRightMerged(long l, long r)
            {
                if (rightStack.Count > 0 && r + 1 == rightStack.Peek().L)
                    r = rightStack.Pop().R;
                rightStack.Push((l, r));
            }

            foreach (var (p, count) in piles)
            {
                for (int k = 0; k < count; k++)
                {
                    while (rightStack.Count > 0 && rightStack.Peek().L <= p)
                    {
                        leftStack.Push(rightStack.Pop());
                    }

                    if (leftStack.Count > 0 && p <= leftStack.Peek().R)
                    {
                        var (segL, segR) = leftStack.Pop();
                        long a = p - segL;
                        long b = segR - p;
                        moves += (a + 1) * (b + 1);

                        long l1 = segL - 1;
                        long r1 = segL + b - 1;
                        long l2 = segL + b + 1;
                        long r2 = segR + 1;

                        PushLeftMerged(l1, r1);
                        if (l2 <= p)
                        {
                            if (rightStack.Count > 0 && r2 + 1 == rightStack.Peek().L)
                                r2 = rightStack.Pop().R;
                            leftStack.Push((l2, r2));
                        }
                        else
                        {
                            PushRightMerged(l2, r2);
                        }
                    }
                    else
                    {
                        long newL = p;
                        long newR = p;
                        if (leftStack.Count > 0 && leftStack.Peek().R + 1 == newL)
                            newL = leftStack.Pop().L;
                        if (rightStack.Count > 0 && newR + 1 == rightStack.Peek().L)
                            newR = rightStack.Pop().R;
                        leftStack.Push((newL, newR));
                    }
                }
            }

            return moves;
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

        #region Question 4 - Different Sum

        private static Func<long> Q4Solver(GCJTestCase test)
        {
            test.Get(out long n, out int b);
            return () => Q4Solver(n, b);
        }

        private static long Q4Solver(long n, int b)
        {
            const long Mod = 1_000_000_007;

            List<int> digits = [];
            long tmp = n;
            while (tmp > 0)
            {
                digits.Add((int)(tmp % b));
                tmp /= b;
            }
            int len = digits.Count;

            long[] fact = new long[b + 1];
            long[,] choose = new long[b + 1, b + 1];
            fact[0] = 1;
            for (int i = 1; i <= b; i++)
                fact[i] = (fact[i - 1] * i) % Mod;

            for (int i = 0; i <= b; i++)
            {
                choose[i, 0] = 1;
                for (int j = 1; j <= i; j++)
                    choose[i, j] = (choose[i - 1, j - 1] + choose[i - 1, j]) % Mod;
            }

            int maxSum = b * (b - 1) / 2;
            long[,] cnt0 = new long[b + 1, maxSum + 1];
            cnt0[0, 0] = 1;
            for (int digit = 1; digit < b; digit++)
            {
                int prevMax = Math.Min(digit, b - 1);
                for (int k = prevMax; k >= 1; k--)
                {
                    for (int s = digit; s <= maxSum; s++)
                    {
                        cnt0[k, s] = (cnt0[k, s] + cnt0[k - 1, s - digit]) % Mod;
                    }
                }
            }

            long Count2(int k, int s, int f)
            {
                if (s < 0 || s > maxSum || k < f || k > b)
                    return 0;
                return cnt0[k - f, s];
            }

            int maxCarry = b / 2;
            long[,,] dp = new long[maxCarry + 1, b + 1, 2];
            for (int carry = 0; carry <= maxCarry; carry++)
            {
                int s = carry * b + digits[0];
                for (int k = 1; k <= b; k++)
                {
                    dp[carry, k, 0] = Count2(k, s, 0);
                    dp[carry, k, 1] = Count2(k, s, 1);
                }
            }

            for (int col = 1; col < len; col++)
            {
                int digit = digits[col];
                long[,] mid = new long[maxCarry + 1, b + 1];
                for (int carry = 0; carry <= maxCarry; carry++)
                {
                    for (int kNext = 0; kNext <= b; kNext++)
                    {
                        long acc = 0;
                        for (int k = kNext; k <= b; k++)
                        {
                            long ways0 = dp[carry, k, 0];
                            if (ways0 != 0)
                                acc = (acc + ways0 * choose[k, kNext]) % Mod;

                            if (kNext >= 1)
                            {
                                long ways1 = dp[carry, k, 1];
                                if (ways1 != 0)
                                    acc = (acc + ways1 * choose[k - 1, kNext - 1]) % Mod;
                            }
                        }
                        mid[carry, kNext] = (acc * fact[kNext]) % Mod;
                    }
                }

                long[,,] nextDp = new long[maxCarry + 1, b + 1, 2];
                for (int carryOut = 0; carryOut <= maxCarry; carryOut++)
                {
                    for (int kNext = 0; kNext <= b; kNext++)
                    {
                        long sum0 = 0;
                        long sum1 = 0;
                        for (int carry = 0; carry <= maxCarry; carry++)
                        {
                            long m = mid[carry, kNext];
                            if (m == 0)
                                continue;
                            int req = carryOut * b + digit - carry;
                            long c0 = Count2(kNext, req, 0);
                            if (c0 != 0)
                                sum0 = (sum0 + m * c0) % Mod;
                            long c1 = Count2(kNext, req, 1);
                            if (c1 != 0)
                                sum1 = (sum1 + m * c1) % Mod;
                        }
                        nextDp[carryOut, kNext, 0] = sum0;
                        nextDp[carryOut, kNext, 1] = sum1;
                    }
                }

                dp = nextDp;
            }

            long total = 0;
            for (int k = 0; k <= b; k++)
            {
                total = (total + dp[0, k, 0]) % Mod;
            }
            return total;
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

        #endregion
    }
}
