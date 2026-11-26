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
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2015
{
    [TestFixture]
    public class FinalsTest : BaseGCJTest
    {
        #region Question 1 - Costly Binary Search

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out string s);
            return () => Q1Solver(s);
        }

        private static int Q1Solver(string s)
        {
            int n = s.Length;
            byte[] digits = new byte[n];
            for (int i = 0; i < n; i++)
                digits[i] = (byte)(s[i] - '0');

            int[][] r = new int[10][];
            int[][] l = new int[10][];
            for (int d = 0; d < 10; d++)
            {
                r[d] = new int[n + 1];
                l[d] = new int[n + 1];
            }

            for (int x = 0; x <= n; x++)
            {
                r[0][x] = x;
                l[0][x] = x;
            }

            for (int c = 1; ; c++)
            {
                int slotC = c % 10;
                int[] curR = r[slotC];
                int[] curL = l[slotC];
                Array.Copy(r[(c - 1) % 10], curR, n + 1);

                for (int i = 0; i < n; i++)
                {
                    int d = digits[i];
                    if (c >= d)
                    {
                        int slot = (c - d) % 10;
                        int x = l[slot][i];
                        int y = r[slot][i + 1];
                        if (y > curR[x])
                            curR[x] = y;
                    }
                }

                for (int x = 1; x <= n; x++)
                {
                    if (curR[x - 1] > curR[x])
                        curR[x] = curR[x - 1];
                }

                if (curR[0] == n)
                    return c;

                int ptr = 0;
                for (int y = 0; y <= n; y++)
                {
                    while (curR[ptr] < y)
                        ptr++;
                    curL[y] = ptr;
                }
            }
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

        #region Question 2 - Campinatorics

        private const long CampBasis = 1_000_000_007L;
        private static readonly Lazy<(long[] Fact, long[] InvFact, long[] DerOverFact)> CampTables = new(() =>
        {
            const int maxN = 1_000_000;
            Modulo m = new(CampBasis);
            m.PrecomputeFactorials(maxN);
            m.PrecomputeDerangements(maxN);

            long[] fact = new long[maxN + 1];
            long[] invFact = new long[maxN + 1];
            long[] derOverFact = new long[maxN + 1];
            for (int i = 0; i <= maxN; i++)
            {
                fact[i] = m.Factorial(i);
                long inv = m.InverseFactorial(i);
                invFact[i] = inv;
                derOverFact[i] = m.Derangements(i) * inv % CampBasis;
            }
            return (fact, invFact, derOverFact);
        });

        private static Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out int x);
            return () => Q2Solver(n, x);
        }

        private static long Q2Solver(int n, int x)
        {
            var (fact, invFact, derOverFact) = CampTables.Value;
            long sum = 0;
            for (int k = x; k <= n; k++)
            {
                sum += derOverFact[n - k] * invFact[k] % CampBasis;
                if (sum >= CampBasis)
                    sum -= CampBasis;
            }

            long fn = fact[n];
            return (fn * fn % CampBasis) * sum % CampBasis;
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

        #region Question 3 - Pretty Good Proportion

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n, out string fStr);
            test.Get(out string s);
            return () => Q3Solver(n, fStr, s);
        }

        private static int Q3Solver(int n, string fStr, string s)
        {
            long fNum = (long)Math.Round(double.Parse(fStr, CultureInfo.InvariantCulture) * 1_000_000.0);
            (long Y, int Index)[] pts = new (long, int)[n + 1];
            pts[0] = (0L, 0);

            long ones = 0;
            for (int i = 1; i <= n; i++)
            {
                if (s[i - 1] == '1')
                    ones++;
                pts[i] = (fNum * i - 1_000_000L * ones, i);
            }

            Array.Sort(pts, static (a, b) =>
            {
                int cmp = a.Y.CompareTo(b.Y);
                return cmp != 0 ? cmp : a.Index.CompareTo(b.Index);
            });

            long bestNum = long.MaxValue;
            long bestDen = 1;
            int bestIdx = n;

            for (int k = 0; k < n; k++)
            {
                long dy = pts[k + 1].Y - pts[k].Y;
                long dx = Math.Abs(pts[k + 1].Index - pts[k].Index);
                int idx = Math.Min(pts[k].Index, pts[k + 1].Index);

                Int128 left = (Int128)dy * bestDen;
                Int128 right = (Int128)bestNum * dx;
                if (left < right || (left == right && idx < bestIdx))
                {
                    bestNum = dy;
                    bestDen = dx;
                    bestIdx = idx;
                }
            }

            return bestIdx;
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

        #region Question 4 - Taking Over The World

        private static Func<int> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m, out int k);
            test.GetLines(m, out int[] u, out int[] v);
            return () => Q4Solver(n, k, u, v);
        }

        private static int Q4Solver(int n, int k, int[] u, int[] v)
        {
            List<int>[] adj = new List<int>[n];
            for (int i = 0; i < n; i++)
                adj[i] = [];
            for (int i = 0; i < u.Length; i++)
            {
                adj[u[i]].Add(v[i]);
                adj[v[i]].Add(u[i]);
            }

            int[] dist = new int[n];
            Array.Fill(dist, -1);
            Queue<int> q = new();
            dist[0] = 0;
            q.Enqueue(0);
            while (q.Count > 0)
            {
                int cur = q.Dequeue();
                if (cur == n - 1)
                    break;
                foreach (int nxt in adj[cur])
                {
                    if (dist[nxt] < 0)
                    {
                        dist[nxt] = dist[cur] + 1;
                        q.Enqueue(nxt);
                    }
                }
            }
            int d0 = dist[n - 1];

            long inf = k + 2;
            MaxFlow flow = new(1);
            const int superSink = 0;
            long totalFlow = 0;
            int prevBase = -1;
            int source = -1;

            for (int x = 0; ; x++)
            {
                int curBase = flow.AddNodes(2 * n);
                if (x == 0)
                    source = curBase;

                for (int i = 0; i < n - 1; i++)
                    flow.AddDirectedEdge(curBase + 2 * i, curBase + 2 * i + 1, 1);

                flow.AddDirectedEdge(curBase + 2 * (n - 1), superSink, inf);

                if (x > 0)
                {
                    for (int i = 0; i < n; i++)
                        flow.AddDirectedEdge(prevBase + 2 * i, curBase + 2 * i, inf);

                    for (int i = 0; i < n - 1; i++)
                    {
                        flow.AddDirectedEdge(prevBase + 2 * i, curBase + 2 * i + 1, inf);
                        int fromTrue = prevBase + 2 * i + 1;
                        foreach (int nxt in adj[i])
                            flow.AddDirectedEdge(fromTrue, curBase + 2 * nxt, inf);
                    }
                }

                prevBase = curBase;
                if (x >= d0)
                {
                    totalFlow += flow.ComputeMaxFlow(source, superSink, (k + 1) - totalFlow);
                    if (totalFlow > k)
                        return x;
                }
            }
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

        #region Question 5 - Merlin QA

        private static Func<int> Q5Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            int[][] spells = new int[n][];
            for (int i = 0; i < n; i++)
                test.Get(out spells[i]);
            return () => Q5Solver(m, spells);
        }

        private static int Q5Solver(int m, int[][] allSpells)
        {
            int baseSum = 0;
            List<int[]> mixedList = [];

            foreach (int[] sp in allSpells)
            {
                bool hasPos = false;
                bool hasNeg = false;
                int sum = 0;
                for (int d = 0; d < m; d++)
                {
                    int v = sp[d];
                    sum += v;
                    if (v > 0) hasPos = true;
                    else if (v < 0) hasNeg = true;
                }

                if (!hasNeg)
                    baseSum += sum;
                else if (hasPos)
                    mixedList.Add(sp);
            }

            if (mixedList.Count == 0 || m <= 1)
                return baseSum;

            int kCount = mixedList.Count;
            int[] flat = new int[kCount * m];
            int[] totalSums = new int[kCount];
            for (int i = 0; i < kCount; i++)
            {
                int sum = 0;
                int rowOff = i * m;
                int[] sp = mixedList[i];
                for (int d = 0; d < m; d++)
                {
                    flat[rowOff + d] = sp[d];
                    sum += sp[d];
                }
                totalSums[i] = sum;
            }

            int[] curArr = new int[m * kCount];
            int[] bestArr = new int[m * kCount];
            int bestMixed = 0;

            void Dfs(int depth, int usedMask)
            {
                int curOff = depth * kCount;
                if (depth == m - 1)
                {
                    int leafTotal = 0;
                    for (int i = 0; i < kCount; i++)
                        leafTotal += Math.Max(bestArr[curOff + i], totalSums[i]);
                    if (leafTotal > bestMixed)
                        bestMixed = leafTotal;
                    return;
                }

                int nextOff = (depth + 1) * kCount;
                for (int dim = 0; dim < m; dim++)
                {
                    if ((usedMask & (1 << dim)) != 0)
                        continue;

                    for (int i = 0; i < kCount; i++)
                    {
                        int nc = curArr[curOff + i] + flat[i * m + dim];
                        int pb = bestArr[curOff + i];
                        curArr[nextOff + i] = nc;
                        bestArr[nextOff + i] = nc > pb ? nc : pb;
                    }
                    Dfs(depth + 1, usedMask | (1 << dim));
                }
            }

            Dfs(0, 0);
            return baseSum + bestMixed;
        }

        [Test]
        public void Q5Sample()
        {
            Test(Q5Solver);
        }

        [Test]
        public void Q5Small()
        {
            Test(Q5Solver);
        }

        [Test]
        public void Q5Large()
        {
            SlowTest(Q5Solver);
        }

        #endregion

        #region Question 6 - Crane Truck

        private const long TapeN = 1L << 40;

        private sealed class SimpleBlock
        {
            public int Shift { get; init; }
            public long Moves { get; init; }
            public int MinOff { get; init; }
            public int MaxOff { get; init; }
            public byte DeltaAtZero { get; init; }
            public (int Off, byte Delta)[] Deltas { get; init; } = [];

            public static SimpleBlock Compile(string segment)
            {
                Dictionary<int, int> diff = [];
                int off = 0;
                long moves = 0;
                foreach (char ch in segment)
                {
                    switch (ch)
                    {
                        case 'f':
                            off++;
                            moves++;
                            break;
                        case 'b':
                            off--;
                            moves++;
                            break;
                        case 'd':
                            diff[off] = (diff.GetValueOrDefault(off) + 1) & 255;
                            break;
                        case 'u':
                            diff[off] = (diff.GetValueOrDefault(off) + 255) & 255;
                            break;
                    }
                }

                List<(int Off, byte Delta)> list = [];
                int minOff = 0;
                int maxOff = 0;
                byte d0 = 0;
                bool any = false;
                foreach (var kvp in diff)
                {
                    byte val = (byte)kvp.Value;
                    if (val == 0)
                        continue;
                    list.Add((kvp.Key, val));
                    if (kvp.Key == 0)
                        d0 = val;
                    if (!any)
                    {
                        minOff = kvp.Key;
                        maxOff = kvp.Key;
                        any = true;
                    }
                    else
                    {
                        if (kvp.Key < minOff) minOff = kvp.Key;
                        if (kvp.Key > maxOff) maxOff = kvp.Key;
                    }
                }

                return new SimpleBlock
                {
                    Shift = off,
                    Moves = moves,
                    MinOff = minOff,
                    MaxOff = maxOff,
                    DeltaAtZero = d0,
                    Deltas = [.. list]
                };
            }

            public SimpleBlock Reversed()
            {
                (int Off, byte Delta)[] rev = new (int, byte)[Deltas.Length];
                for (int i = 0; i < Deltas.Length; i++)
                    rev[i] = (-Deltas[i].Off, Deltas[i].Delta);
                return new SimpleBlock
                {
                    Shift = -Shift,
                    Moves = Moves,
                    MinOff = -MaxOff,
                    MaxOff = -MinOff,
                    DeltaAtZero = DeltaAtZero,
                    Deltas = rev
                };
            }
        }

        private sealed class TapeState
        {
            public byte[] Win = [0];
            public long WinL = 0;
            public long WinR = 1;
            public byte[] Bg1 = [0];
            public int P1 = 1;
            public byte[] Bg2 = [0];
            public int P2 = 1;

            public byte GetBg(long offsetFromWinR)
            {
                int i1 = (int)(offsetFromWinR % P1);
                if (i1 < 0) i1 += P1;
                int i2 = (int)(offsetFromWinR % P2);
                if (i2 < 0) i2 += P2;
                return (byte)(Bg1[i1] + Bg2[i2]);
            }

            public void EnsureRange(long minIdx, long maxIdxExclusive)
            {
                if (minIdx < WinL)
                {
                    long pad = WinL - minIdx;
                    long arcLen = TapeN - (WinR - WinL);
                    byte[] next = new byte[(int)(pad + (WinR - WinL))];
                    for (long j = 0; j < pad; j++)
                        next[(int)j] = GetBg(arcLen - pad + j);
                    Array.Copy(Win, 0, next, (int)pad, Win.Length);
                    Win = next;
                    WinL = minIdx;
                }

                if (maxIdxExclusive > WinR)
                {
                    long pad = maxIdxExclusive - WinR;
                    byte[] next = new byte[Win.Length + (int)pad];
                    Array.Copy(Win, 0, next, 0, Win.Length);
                    for (long j = 0; j < pad; j++)
                        next[Win.Length + (int)j] = GetBg(j);
                    Win = next;
                    RotateBgRight(pad);
                    WinR = maxIdxExclusive;
                }
            }

            public void RotateBgRight(long shift)
            {
                Bg1 = RotateArray(Bg1, P1, shift);
                Bg2 = RotateArray(Bg2, P2, shift);
            }

            private static byte[] RotateArray(byte[] arr, int p, long shift)
            {
                if (p <= 1)
                    return arr;
                int s = (int)(shift % p);
                if (s < 0) s += p;
                if (s == 0)
                    return arr;
                byte[] next = new byte[p];
                for (int i = 0; i < p; i++)
                    next[i] = arr[(i + s) % p];
                return next;
            }

            public void PrependBuffer(byte[] buf, int bufLen, long bufStartWrapped)
            {
                long pad = WinL - bufStartWrapped;
                byte[] next = new byte[(int)pad + Win.Length];
                Array.Copy(buf, 0, next, 0, (int)pad);
                Array.Copy(Win, 0, next, (int)pad, Win.Length);
                Win = next;
                WinL = bufStartWrapped;
            }

            public void AppendBuffer(byte[] buf, int appendCount)
            {
                if (appendCount <= 0)
                    return;
                byte[] next = new byte[Win.Length + appendCount];
                Array.Copy(Win, 0, next, 0, Win.Length);
                Array.Copy(buf, 0, next, Win.Length, appendCount);
                Win = next;
                RotateBgRight(appendCount);
                WinR += appendCount;
            }

            public void MergeBg2IntoBg1()
            {
                if (P1 == 1)
                {
                    byte b1 = Bg1[0];
                    if (b1 != 0)
                    {
                        for (int i = 0; i < P2; i++)
                            Bg2[i] = (byte)(Bg2[i] + b1);
                    }
                    Bg1 = Bg2;
                    P1 = P2;
                    Bg2 = [0];
                    P2 = 1;
                }
            }

            public void Reverse(ref long pos)
            {
                long arcLen = TapeN - (WinR - WinL);
                Array.Reverse(Win);
                long newWinL = 1 - WinR;
                long newWinR = 1 - WinL;
                WinL = newWinL;
                WinR = newWinR;

                Bg1 = ReverseBg(Bg1, P1, arcLen);
                Bg2 = ReverseBg(Bg2, P2, arcLen);

                long nPos = -pos;
                long rel = ((nPos - WinL) % TapeN + TapeN) % TapeN;
                pos = WinL + rel;
            }

            private static byte[] ReverseBg(byte[] arr, int p, long arcLen)
            {
                if (p <= 1)
                    return arr;
                byte[] next = new byte[p];
                for (int j = 0; j < p; j++)
                {
                    int idx = (int)(((arcLen - 1 - j) % p + p) % p);
                    next[j] = arr[idx];
                }
                return next;
            }
        }

        private static UInt128 ExecuteDirect(TapeState state, ref long pos, SimpleBlock block)
        {
            long lo = Math.Min(pos + block.MinOff, pos + block.Shift);
            long hi = Math.Max(pos + block.MaxOff, pos + block.Shift) + 1;
            state.EnsureRange(lo, hi);

            foreach (var (off, delta) in block.Deltas)
                state.Win[pos + off - state.WinL] += delta;

            pos += block.Shift;
            return (UInt128)block.Moves;
        }

        private static UInt128 ExecuteLoop(TapeState state, ref long pos, SimpleBlock block, bool needStateAfter)
        {
            UInt128 totalMoves = 0;
            if (block.Shift == 0)
            {
                state.EnsureRange(pos + block.MinOff, pos + block.MaxOff + 1);
                byte val = (byte)(state.Win[pos - state.WinL] + block.DeltaAtZero);
                int iters = 1;
                while (val != 0)
                {
                    val += block.DeltaAtZero;
                    iters++;
                }

                byte mult = (byte)(iters & 255);
                if (mult != 0)
                {
                    foreach (var (off, delta) in block.Deltas)
                        state.Win[pos + off - state.WinL] += (byte)(mult * delta);
                }

                return (UInt128)iters * (UInt128)block.Moves;
            }

            bool reversed = block.Shift < 0;
            if (reversed)
            {
                state.Reverse(ref pos);
                block = block.Reversed();
            }

            int k = block.Shift;
            int s = Math.Max(Math.Abs(block.MinOff), Math.Abs(block.MaxOff));
            state.EnsureRange(pos - s, pos + s + k + 1);
            if (state.WinR - state.WinL < 3L * s + k + 2)
            {
                long targetR = Math.Max(state.WinR, state.WinL + 3L * s + k + 2);
                state.EnsureRange(state.WinL, targetR);
            }

            byte[] sweepDelta = new byte[k];
            byte testDelta = 0;
            foreach (var (off, delta) in block.Deltas)
            {
                int r = off % k;
                if (r < 0) r += k;
                sweepDelta[r] += delta;
                if (off > 0 && off % k == 0)
                    testDelta += delta;
            }

            List<(int Rem, byte Delta)> nonZeroSweepList = [];
            for (int r = 0; r < k; r++)
            {
                if (sweepDelta[r] != 0)
                    nonZeroSweepList.Add((r, sweepDelta[r]));
            }
            (int Rem, byte Delta)[] nonZeroSweep = [.. nonZeroSweepList];

            state.Bg2 = new byte[k];
            state.P2 = k;

            int p1 = state.P1;
            int g = (int)MathFormulas.Gcd(p1, k);
            int cycleLen = p1 / g;
            bool[,] hasVal = new bool[g, 256];
            for (int idx = 0; idx < p1; idx++)
                hasVal[idx % g, state.Bg1[idx]] = true;

            int exitCap = 6 * s + 5 * k + 32;
            byte[] exitBuf = new byte[exitCap];
            int[] exitStamp = new int[exitCap];
            int exitGen = 0;

            int entryCap = 4 * s + 3 * k + 32;
            byte[] entryBuf = new byte[entryCap];
            int[] entryStamp = new int[entryCap];
            int entryGen = 0;

            while (true)
            {
                long winL = state.WinL;
                long winR = state.WinR;
                byte[] win = state.Win;

                exitGen++;
                long pStart = -1;
                int maxExitTouched = 0;
                bool stoppedInPhase1 = false;

                while (true)
                {
                    if (pStart < 0 && pos >= winR + s)
                        pStart = pos;
                    if (pStart >= 0 && pos - pStart >= 2L * s + k)
                        break;

                    totalMoves += (UInt128)block.Moves;
                    foreach (var (off, delta) in block.Deltas)
                    {
                        long idx = pos + off;
                        if (idx < winR)
                        {
                            win[idx - winL] += delta;
                        }
                        else
                        {
                            int ei = (int)(idx - winR);
                            if (exitStamp[ei] != exitGen)
                            {
                                exitStamp[ei] = exitGen;
                                exitBuf[ei] = state.GetBg(ei);
                            }
                            exitBuf[ei] += delta;
                            if (ei + 1 > maxExitTouched)
                                maxExitTouched = ei + 1;
                        }
                    }
                    pos += k;
                    byte tVal;
                    if (pos < winR)
                    {
                        tVal = win[pos - winL];
                    }
                    else
                    {
                        int ei = (int)(pos - winR);
                        if (exitStamp[ei] != exitGen)
                        {
                            exitStamp[ei] = exitGen;
                            exitBuf[ei] = state.GetBg(ei);
                        }
                        tVal = exitBuf[ei];
                    }
                    if (tVal == 0)
                    {
                        stoppedInPhase1 = true;
                        break;
                    }
                }

                if (stoppedInPhase1)
                {
                    if (needStateAfter)
                    {
                        int needAppend = Math.Max(maxExitTouched, pos >= winR ? (int)(pos - winR + 1) : 0);
                        for (int j = 0; j < needAppend; j++)
                        {
                            if (exitStamp[j] != exitGen)
                                exitBuf[j] = state.GetBg(j);
                        }
                        state.AppendBuffer(exitBuf, needAppend);
                    }
                    break;
                }

                long pB = pos;
                long limitC = winL + TapeN - 2L * s - k;
                long tB = (limitC - pB) / k;
                long pC = pB + tB * k;

                long u0 = pB + k - winR;
                byte bg2Val = state.Bg2[(int)(u0 % k)];
                byte target = (byte)-(bg2Val + testDelta);
                int coset = (int)(u0 % g);

                if (hasVal[coset, target])
                {
                    long maxCheck = Math.Min(tB, cycleLen);
                    long hitT = -1;
                    for (long t = 0; t < maxCheck; t++)
                    {
                        if (state.Bg1[(int)((u0 + t * k) % p1)] == target)
                        {
                            hitT = t;
                            break;
                        }
                    }

                    if (hitT >= 0)
                    {
                        totalMoves += (UInt128)(hitT + 1) * (UInt128)block.Moves;
                        pos = pB + (hitT + 1) * k;
                        if (needStateAfter)
                        {
                            // When needStateAfter is true (B followed by C/D), P1 == 1 so hitT == 0.
                            foreach (var (off, delta) in block.Deltas)
                            {
                                long idx = pB + off;
                                int ei = (int)(idx - winR);
                                if (exitStamp[ei] != exitGen)
                                {
                                    exitStamp[ei] = exitGen;
                                    exitBuf[ei] = state.GetBg(ei);
                                }
                                exitBuf[ei] += delta;
                                if (ei + 1 > maxExitTouched)
                                    maxExitTouched = ei + 1;
                            }
                            int needAppend = Math.Max(maxExitTouched, (int)(pos - winR + 1));
                            for (int j = 0; j < needAppend; j++)
                            {
                                if (exitStamp[j] != exitGen)
                                    exitBuf[j] = state.GetBg(j);
                            }
                            state.AppendBuffer(exitBuf, needAppend);
                        }
                        break;
                    }
                }

                totalMoves += (UInt128)tB * (UInt128)block.Moves;

                long entryL = pC - s;
                long arcEnd = winL + TapeN;
                int entryLen = (int)(arcEnd - entryL);
                entryGen++;

                long backSteps = (2L * s + k - 1) / k + 2;
                long startPreT = Math.Max(0L, tB - backSteps);
                for (long t = startPreT; t < tB; t++)
                {
                    long pPre = pB + t * k;
                    foreach (var (off, delta) in block.Deltas)
                    {
                        long idx = pPre + off;
                        if (idx >= entryL && idx < arcEnd)
                        {
                            int ei = (int)(idx - entryL);
                            if (entryStamp[ei] != entryGen)
                            {
                                entryStamp[ei] = entryGen;
                                entryBuf[ei] = state.GetBg(entryL + ei - winR);
                            }
                            entryBuf[ei] += delta;
                        }
                    }
                }

                pos = pC;
                bool stoppedInPartC = false;
                while (pos < arcEnd + s)
                {
                    totalMoves += (UInt128)block.Moves;
                    foreach (var (off, delta) in block.Deltas)
                    {
                        long idx = pos + off;
                        if (idx < arcEnd)
                        {
                            int ei = (int)(idx - entryL);
                            if (entryStamp[ei] != entryGen)
                            {
                                entryStamp[ei] = entryGen;
                                entryBuf[ei] = state.GetBg(entryL + ei - winR);
                            }
                            entryBuf[ei] += delta;
                        }
                        else
                        {
                            win[(int)(idx - TapeN - winL)] += delta;
                        }
                    }

                    pos += k;
                    byte tVal;
                    if (pos < arcEnd)
                    {
                        int ei = (int)(pos - entryL);
                        if (entryStamp[ei] != entryGen)
                        {
                            entryStamp[ei] = entryGen;
                            entryBuf[ei] = state.GetBg(entryL + ei - winR);
                        }
                        tVal = entryBuf[ei];
                    }
                    else
                    {
                        tVal = win[(int)(pos - TapeN - winL)];
                    }
                    if (tVal == 0)
                    {
                        stoppedInPartC = true;
                        break;
                    }
                }

                if (stoppedInPartC)
                {
                    if (needStateAfter)
                    {
                        for (int j = 0; j < entryLen; j++)
                        {
                            if (entryStamp[j] != entryGen)
                                entryBuf[j] = state.GetBg(entryL + j - winR);
                        }
                    }
                }

                int bgShift = (int)(((pB - winR) % k + k) % k);
                byte[] bg2 = state.Bg2;
                for (int i = 0; i < nonZeroSweep.Length; i++)
                {
                    int u = nonZeroSweep[i].Rem + bgShift;
                    if (u >= k)
                        u -= k;
                    bg2[u] += nonZeroSweep[i].Delta;
                }

                if (stoppedInPartC)
                {
                    if (needStateAfter)
                    {
                        pos -= TapeN;
                        state.PrependBuffer(entryBuf, entryLen, entryL - TapeN);
                    }
                    break;
                }

                pos -= TapeN;
            }

            if (reversed && needStateAfter)
                state.Reverse(ref pos);

            return totalMoves;
        }

        private static Func<string> Q6Solver(GCJTestCase test)
        {
            test.Get(out string prog);
            return () => Q6Solver(prog);
        }

        private static string Q6Solver(string prog)
        {
            int open1 = prog.IndexOf('(');
            if (open1 < 0)
                return SimpleBlock.Compile(prog).Moves.ToString(CultureInfo.InvariantCulture);

            int close1 = prog.IndexOf(')', open1 + 1);
            int open2 = prog.IndexOf('(', close1 + 1);

            string strA = prog[..open1];
            string strB = prog.Substring(open1 + 1, close1 - open1 - 1);
            SimpleBlock blockA = SimpleBlock.Compile(strA);
            SimpleBlock blockB = SimpleBlock.Compile(strB);

            TapeState state = new();
            long pos = 0;
            UInt128 totalMoves = ExecuteDirect(state, ref pos, blockA);

            if (open2 < 0)
            {
                totalMoves += ExecuteLoop(state, ref pos, blockB, needStateAfter: false);
                SimpleBlock blockC = SimpleBlock.Compile(prog[(close1 + 1)..]);
                totalMoves += (UInt128)blockC.Moves;
                return totalMoves.ToString(CultureInfo.InvariantCulture);
            }

            int close2 = prog.IndexOf(')', open2 + 1);
            SimpleBlock blockMid = SimpleBlock.Compile(prog.Substring(close1 + 1, open2 - close1 - 1));
            SimpleBlock blockD = SimpleBlock.Compile(prog.Substring(open2 + 1, close2 - open2 - 1));
            SimpleBlock blockE = SimpleBlock.Compile(prog[(close2 + 1)..]);

            totalMoves += ExecuteLoop(state, ref pos, blockB, needStateAfter: true);
            state.MergeBg2IntoBg1();
            totalMoves += ExecuteDirect(state, ref pos, blockMid);
            totalMoves += ExecuteLoop(state, ref pos, blockD, needStateAfter: false);
            totalMoves += (UInt128)blockE.Moves;

            return totalMoves.ToString(CultureInfo.InvariantCulture);
        }

        [Test]
        public void Q6Sample()
        {
            Test(Q6Solver);
        }

        [Test]
        public void Q6Small()
        {
            Test(Q6Solver);
        }

        [Test]
        public void Q6Large()
        {
            SlowTest(Q6Solver);
        }

        #endregion
    }
}
