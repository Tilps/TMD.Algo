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
using System.Threading;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2009
{
    [TestFixture]
    public class R1ATest : BaseGCJTest
    {
        #region Question 1 - Multi-base happiness

        private static readonly Lock HappyLock = new();
        private static int[]? minHappyByMask;

        private static int[] GetMinHappyByMask()
        {
            if (minHappyByMask != null)
                return minHappyByMask;

            lock (HappyLock)
            {
                if (minHappyByMask != null)
                    return minHappyByMask;

                const int SmallLimit = 1000;
                bool[,] happySmall = new bool[11, SmallLimit];
                int[] chunkPow = new int[11];
                int[,] sumSqChunk = new int[11, 10000];

                for (int b = 2; b <= 10; b++)
                {
                    byte[] state = new byte[SmallLimit]; // 0 = unvisited, 1 = visiting, 2 = happy, 3 = unhappy
                    state[1] = 2;
                    happySmall[b, 1] = true;
                    List<int> path = [];

                    for (int i = 2; i < SmallLimit; i++)
                    {
                        if (state[i] != 0)
                            continue;

                        path.Clear();
                        int cur = i;
                        while (state[cur] == 0)
                        {
                            state[cur] = 1;
                            path.Add(cur);
                            int next = 0;
                            foreach (int digit in ((long)cur).ToDigits(b))
                            {
                                next += digit * digit;
                            }
                            cur = next;
                        }

                        byte resultState = state[cur] == 2 ? (byte)2 : (byte)3;
                        bool isHappy = resultState == 2;
                        foreach (int node in path)
                        {
                            state[node] = resultState;
                            happySmall[b, node] = isHappy;
                        }
                    }

                    int p = 1;
                    while (p * b <= 10000)
                        p *= b;
                    chunkPow[b] = p;
                    for (int v = 1; v < p; v++)
                    {
                        sumSqChunk[b, v] = sumSqChunk[b, v / b] + (v % b) * (v % b);
                    }
                }

                bool IsHappy(int x, int b)
                {
                    int p = chunkPow[b];
                    while (x >= SmallLimit)
                    {
                        int s = 0;
                        int t = x;
                        while (t > 0)
                        {
                            s += sumSqChunk[b, t % p];
                            t /= p;
                        }
                        x = s;
                    }
                    return happySmall[b, x];
                }

                int[] table = new int[1 << 9];
                for (int mask = 1; mask < table.Length; mask++)
                {
                    int start = 2;
                    int temp = mask;
                    while (temp != 0)
                    {
                        int lsb = BitTricks.KeepLeastSignificantSetBit(temp);
                        int sub = table[mask ^ lsb];
                        if (sub > start)
                            start = sub;
                        temp ^= lsb;
                    }

                    int candidate = start;
                    while (true)
                    {
                        bool ok = true;
                        for (int b = 10; b >= 2; b--)
                        {
                            if ((mask & (1 << (b - 2))) != 0 && !IsHappy(candidate, b))
                            {
                                ok = false;
                                break;
                            }
                        }
                        if (ok)
                            break;
                        candidate++;
                    }
                    table[mask] = candidate;
                }

                minHappyByMask = table;
                return minHappyByMask;
            }
        }

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int[] bases);
            return () => Q1Solver(bases);
        }

        private static int Q1Solver(int[] bases)
        {
            int mask = 0;
            foreach (int b in bases)
            {
                mask |= 1 << (b - 2);
            }
            return GetMinHappyByMask()[mask];
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

        #region Question 2 - Crossing the Road

        private static Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            int[] s = new int[n * m];
            int[] w = new int[n * m];
            int[] t = new int[n * m];

            for (int r = 0; r < n; r++)
            {
                test.Get(out int[] rowData);
                for (int c = 0; c < m; c++)
                {
                    int idx = r * m + c;
                    s[idx] = rowData[3 * c];
                    w[idx] = rowData[3 * c + 1];
                    t[idx] = rowData[3 * c + 2];
                }
            }

            return () => Q2Solver(n, m, s, w, t);
        }

        private static long Q2Solver(int n, int m, int[] s, int[] w, int[] t)
        {
            int maxRy = 2 * n;
            int maxCx = 2 * m;
            (int Ry, int Cx) start = (maxRy - 1, 0);
            (int Ry, int Cx) target = (0, maxCx - 1);

            long CrossWait(long currentTime, int interR, int interC, bool northSouth)
            {
                int idx = interR * m + interC;
                long nsDur = s[idx];
                long ewDur = w[idx];
                long cycle = nsDur + ewDur;
                long phase = ((currentTime - t[idx]) % cycle + cycle) % cycle;

                if (northSouth)
                    return phase < nsDur ? 0L : cycle - phase;

                return phase >= nsDur ? 0L : nsDur - phase;
            }

            IEnumerable<KeyValuePair<(int Ry, int Cx), long>> Transitions((int Ry, int Cx) pos, long time)
            {
                int ry = pos.Ry;
                int cx = pos.Cx;

                // Vertical moves
                if (ry > 0)
                {
                    bool crossStreet = (ry & 1) == 1;
                    long cost = crossStreet ? CrossWait(time, ry >> 1, cx >> 1, northSouth: true) + 1L : 2L;
                    yield return new KeyValuePair<(int, int), long>((ry - 1, cx), time + cost);
                }
                if (ry + 1 < maxRy)
                {
                    bool crossStreet = (ry & 1) == 0;
                    long cost = crossStreet ? CrossWait(time, ry >> 1, cx >> 1, northSouth: true) + 1L : 2L;
                    yield return new KeyValuePair<(int, int), long>((ry + 1, cx), time + cost);
                }

                // Horizontal moves
                if (cx > 0)
                {
                    bool crossStreet = (cx & 1) == 1;
                    long cost = crossStreet ? CrossWait(time, ry >> 1, cx >> 1, northSouth: false) + 1L : 2L;
                    yield return new KeyValuePair<(int, int), long>((ry, cx - 1), time + cost);
                }
                if (cx + 1 < maxCx)
                {
                    bool crossStreet = (cx & 1) == 0;
                    long cost = crossStreet ? CrossWait(time, ry >> 1, cx >> 1, northSouth: false) + 1L : 2L;
                    yield return new KeyValuePair<(int, int), long>((ry, cx + 1), time + cost);
                }
            }

            Search.TryPfs(start, 0L, pos => pos == target, Transitions, out long result);
            return result;
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

        #region Question 3 - Collecting Cards

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int c, out int n);
            return () => Q3Solver(c, n);
        }

        private static long SafeChoose(int n, int k, ref long[,] cache)
        {
            if (k < 0 || k > n)
                return 0;
            return Combinatorics.Choose(n, k, ref cache);
        }

        private static string Q3Solver(int c, int n)
        {
            long[,] chooseCache = null!;
            Combinatorics.Choose(c, n, ref chooseCache);
            double totalPacks = SafeChoose(c, n, ref chooseCache);

            double[] expected = new double[c + 1];
            expected[c] = 0.0;

            for (int owned = c - 1; owned >= 0; owned--)
            {
                double pSame = (SafeChoose(owned, n, ref chooseCache) * 1.0) / totalPacks;
                double sumHigher = 1.0;
                int maxNew = Math.Min(n, c - owned);
                for (int gain = 1; gain <= maxNew; gain++)
                {
                    double ways = (double)SafeChoose(c - owned, gain, ref chooseCache) *
                                  SafeChoose(owned, n - gain, ref chooseCache);
                    sumHigher += (ways / totalPacks) * expected[owned + gain];
                }
                expected[owned] = sumHigher / (1.0 - pSame);
            }

            return $"{expected[0]:F7}";
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
