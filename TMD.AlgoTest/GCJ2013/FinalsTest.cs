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
using System.Numerics;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2013
{
    [TestFixture]
    public partial class FinalsTest : BaseGCJTest
    {
        #region Question 1 - Graduation Requirements

        private static Func<long> Q1Solver(GCJTestCase test)
        {
            test.Get(out int c);
            test.Get(out long x, out long n);
            (long S, long E, long T)[] cars = new (long, long, long)[c];
            for (int i = 0; i < c; i++)
            {
                test.Get(out long s, out long e, out long t);
                cars[i] = (s - 1, e - 1, t);
            }
            return () => Q1Solver(c, x, n, cars);
        }

        private static long Q1Solver(int c, long x, long n, (long S, long E, long T)[] cars)
        {
            long[] mArr = new long[c];
            long[] twoTArr = new long[c];
            long[] twoEndArr = new long[c];
            long[] baseModArr = new long[c];
            long[] endPosArr = new long[c];
            long[] endTimeArr = new long[c];

            for (int i = 0; i < c; i++)
            {
                var (s, e, t) = cars[i];
                long d = (e - s + n) % n;
                mArr[i] = ((s - t) % n + n) % n;
                long twoT = 2L * t;
                twoTArr[i] = twoT;
                twoEndArr[i] = 2L * (t + d);
                baseModArr[i] = twoT % n;
                endPosArr[i] = e;
                endTimeArr[i] = t + d;
            }

            long twoX = 2L * x;
            long best = 0;

            void Evaluate(long diagK, long tCand)
            {
                long twoCand = 2L * tCand;
                long maxBelow = -1L;
                long minAbove = twoX + 1L;

                for (int i = 0; i < c; i++)
                {
                    long r = diagK - mArr[i];
                    if (r < 0)
                        r += n;
                    long rem = r - baseModArr[i];
                    if (rem < 0)
                        rem += n;

                    long twoTau = twoTArr[i] + rem;
                    long twoEnd = twoEndArr[i];
                    if (twoTau <= twoEnd)
                    {
                        if (twoTau <= twoCand && twoTau > maxBelow)
                            maxBelow = twoTau;
                        if (twoTau >= twoCand && twoTau < minAbove)
                            minAbove = twoTau;

                        twoTau += n;
                        if (twoTau <= twoEnd)
                        {
                            if (twoTau <= twoCand && twoTau > maxBelow)
                                maxBelow = twoTau;
                            if (twoTau >= twoCand && twoTau < minAbove)
                                minAbove = twoTau;
                        }
                    }
                }

                long a = (maxBelow + 2L) >> 1;
                long b = (minAbove - 1L) >> 1;
                if (b >= a)
                {
                    long dur = b - a;
                    if (dur > best)
                        best = dur;
                }
            }

            Evaluate(0L, 0L);
            Evaluate(1L % n, 0L);

            void TryAround(long p, long t)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;
                        long diagK = ((p + dx + t + dy) % n + n) % n;
                        long tCand = Math.Clamp(t + dy, 0L, x);
                        Evaluate(diagK, tCand);
                    }
                }
            }

            for (int i = 0; i < c; i++)
            {
                TryAround(cars[i].S, cars[i].T);
                TryAround(endPosArr[i], endTimeArr[i]);
            }

            return best;
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

        #region Question 2 - Drummer

        private static Func<double> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out long[] t);
            return () => Q2Solver(n, t);
        }

        private static double Q2Solver(int n, long[] t)
        {
            Point2L[] pts = new Point2L[n];
            for (int i = 0; i < n; i++)
                pts[i] = new Point2L(i, t[i]);
            return Geometry.MinimumVerticalStripWidth(pts) * 0.5;
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

        #region Question 3 - X Marks the Spot

        private const long CoordScale = 1_000_000_000L;
        private const long DirScale = 10_000_000_000_000_000L;

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            (int X, int Y)[] mines = new (int, int)[4 * n];
            for (int i = 0; i < mines.Length; i++)
                test.Get(out mines[i]);
            return () => Q3Solver(n, mines);
        }

        private static (long Ux, long Uy) DirectionFor(long s) =>
            s <= DirScale ? (DirScale, s) : (2L * DirScale - s, DirScale);

        private static BigInteger RoundDiv(BigInteger num, BigInteger den)
        {
            bool neg = (num.Sign ^ den.Sign) < 0;
            BigInteger aNum = BigInteger.Abs(num);
            BigInteger aDen = BigInteger.Abs(den);
            BigInteger q = (aNum + (aDen >> 1)) / aDen;
            return neg ? -q : q;
        }

        private static string FormatFixedPoint(long scaled)
        {
            if (scaled == 0)
                return "0";
            bool neg = scaled < 0;
            long abs = Math.Abs(scaled);
            long ip = abs / CoordScale;
            long fp = abs % CoordScale;
            if (fp == 0)
                return neg ? $"-{ip}" : ip.ToString(CultureInfo.InvariantCulture);
            string frac = fp.ToString("D9", CultureInfo.InvariantCulture).TrimEnd('0');
            return $"{(neg ? "-" : "")}{ip}.{frac}";
        }

        private static bool VerifyScaledX(
            (int X, int Y)[] mines,
            int n,
            long ax, long ay, long bx, long by)
        {
            if (ax == bx && ay == by)
                return false;
            if (Math.Abs(ax) > 1_000_000_000_000_000_000L ||
                Math.Abs(ay) > 1_000_000_000_000_000_000L ||
                Math.Abs(bx) > 1_000_000_000_000_000_000L ||
                Math.Abs(by) > 1_000_000_000_000_000_000L)
                return false;

            Int128 v1x = bx - ax;
            Int128 v1y = by - ay;
            Int128 v2x = -v1y;
            Int128 v2y = v1x;

            int q0 = 0, q1 = 0, q2 = 0, q3 = 0;
            foreach (var (mx, my) in mines)
            {
                Int128 px = (Int128)mx * CoordScale - ax;
                Int128 py = (Int128)my * CoordScale - ay;
                Int128 cp1 = v1x * py - v1y * px;
                Int128 cp2 = v2x * py - v2y * px;
                if (cp1 == 0 || cp2 == 0)
                    return false;
                int q = (cp1 < 0 ? 2 : 0) + (cp2 < 0 ? 1 : 0);
                switch (q)
                {
                    case 0: q0++; break;
                    case 1: q1++; break;
                    case 2: q2++; break;
                    default: q3++; break;
                }
            }
            return q0 == n && q1 == n && q2 == n && q3 == n;
        }

        private static string Q3Solver(int n, (int X, int Y)[] mines)
        {
            int total = 4 * n;
            Int128[] c1 = new Int128[total];
            Int128[] c2 = new Int128[total];
            Int128[] s1 = new Int128[total];
            Int128[] s2 = new Int128[total];

            bool TryEvaluate(long s, out int qCount, out Int128 m1Double, out Int128 m2Double)
            {
                var (ux, uy) = DirectionFor(s);
                Int128 bux = ux;
                Int128 buy = uy;
                for (int i = 0; i < total; i++)
                {
                    long mx = mines[i].X;
                    long my = mines[i].Y;
                    Int128 v1 = bux * my - buy * mx;
                    Int128 v2 = -(bux * mx + buy * my);
                    c1[i] = v1;
                    c2[i] = v2;
                    s1[i] = v1;
                    s2[i] = v2;
                }
                Array.Sort(s1);
                Array.Sort(s2);

                if (s1[2 * n - 1] == s1[2 * n] || s2[2 * n - 1] == s2[2 * n])
                {
                    qCount = -1;
                    m1Double = 0;
                    m2Double = 0;
                    return false;
                }

                Int128 split1 = s1[2 * n - 1];
                Int128 split2 = s2[2 * n - 1];
                int cnt = 0;
                for (int i = 0; i < total; i++)
                {
                    if (c1[i] > split1 && c2[i] > split2)
                        cnt++;
                }

                qCount = cnt;
                m1Double = s1[2 * n - 1] + s1[2 * n];
                m2Double = s2[2 * n - 1] + s2[2 * n];
                return true;
            }

            long lo = 1L;
            long hi = 2L * DirScale + 1L;
            TryEvaluate(lo, out int qLo, out Int128 m1Good, out Int128 m2Good);
            long sGood = lo;

            if (qLo != n)
            {
                bool wantInc = qLo < n;
                lo++;
                hi--;
                while (lo <= hi)
                {
                    long mid = lo + ((hi - lo) >> 1);
                    long probe = mid;
                    if (!TryEvaluate(probe, out int qMid, out m1Good, out m2Good))
                    {
                        probe = mid + 1 <= hi ? mid + 1 : mid - 1;
                        TryEvaluate(probe, out qMid, out m1Good, out m2Good);
                    }

                    if (qMid == n)
                    {
                        sGood = probe;
                        break;
                    }

                    bool goRight = wantInc ? (qMid < n) : (qMid > n);
                    if (goRight)
                        lo = probe + 1;
                    else
                        hi = probe - 1;
                }
            }

            bool TryBuildCoordinates(
                long s,
                Int128 m1Dbl,
                Int128 m2Dbl,
                out long ax, out long ay, out long bx, out long by)
            {
                var (ux, uy) = DirectionFor(s);
                BigInteger bux = ux;
                BigInteger buy = uy;
                BigInteger bm1 = (BigInteger)m1Dbl;
                BigInteger bm2 = (BigInteger)m2Dbl;
                BigInteger denom = 2 * (bux * bux + buy * buy);

                BigInteger numX = -(bux * bm2 + buy * bm1) * CoordScale;
                BigInteger numY = (bux * bm1 - buy * bm2) * CoordScale;
                ax = (long)RoundDiv(numX, denom);
                ay = (long)RoundDiv(numY, denom);

                BigInteger step = 900_000_000_000_000_000L;
                bx = ax + (long)(step * bux / DirScale);
                by = ay + (long)(step * buy / DirScale);
                return VerifyScaledX(mines, n, ax, ay, bx, by);
            }

            if (!TryBuildCoordinates(sGood, m1Good, m2Good, out long finalAx, out long finalAy, out long finalBx, out long finalBy))
            {
                bool found = false;
                for (int bit = 0; bit <= 53 && !found; bit++)
                {
                    long delta = 1L << bit;
                    foreach (long candS in new[] { sGood + delta, sGood - delta })
                    {
                        if (candS < 0 || candS > 3L * DirScale)
                            continue;
                        if (TryEvaluate(candS, out int qc, out Int128 cm1, out Int128 cm2) &&
                            qc == n &&
                            TryBuildCoordinates(candS, cm1, cm2, out finalAx, out finalAy, out finalBx, out finalBy))
                        {
                            found = true;
                            break;
                        }
                    }
                }
                Assert.IsTrue(found, "Failed to back off from discontinuity.");
            }

            return $"{FormatFixedPoint(finalAx)} {FormatFixedPoint(finalAy)} {FormatFixedPoint(finalBx)} {FormatFixedPoint(finalBy)}";
        }

        [GeneratedRegex(@"^(-)?(\d*)(\.(\d*))?$")]
        private static partial Regex FixedPointRegex();

        private static long ParseFixedPointStrict(string token)
        {
            Match m = FixedPointRegex().Match(token);
            Assert.IsTrue(m.Success, $"Invalid fixed-point format '{token}'");
            string intPart = m.Groups[2].Value;
            string fracPart = m.Groups[4].Value;
            Assert.IsTrue(intPart.Length + fracPart.Length >= 1, $"Empty numeric token '{token}'");
            Assert.IsTrue(fracPart.Length <= 9, $"Too many decimals in '{token}'");

            long whole = intPart.Length == 0 ? 0L : long.Parse(intPart, CultureInfo.InvariantCulture);
            long frac = fracPart.Length == 0 ? 0L : long.Parse(fracPart.PadRight(9, '0'), CultureInfo.InvariantCulture);
            long val = whole * CoordScale + frac;
            return m.Groups[1].Success ? -val : val;
        }

        private static readonly char[] LineSeps = ['\r', '\n'];
        private static readonly char[] TokenSeps = [' ', '\t'];

        private static void ValidateQ3(string input, string expectedOutput, List<string> results)
        {
            string[] inLines = input.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            int idx = 0;
            int t = int.Parse(inLines[idx++]);
            Assert.AreEqual(t, results.Count);

            for (int tc = 0; tc < t; tc++)
            {
                int n = int.Parse(inLines[idx++]);
                (int X, int Y)[] mines = new (int, int)[4 * n];
                for (int i = 0; i < 4 * n; i++)
                {
                    string[] tok = inLines[idx++].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
                    mines[i] = (int.Parse(tok[0]), int.Parse(tok[1]));
                }

                string[] actTok = results[tc].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(6, actTok.Length, $"Case #{tc + 1} expected 4 coordinates");
                long ax = ParseFixedPointStrict(actTok[2]);
                long ay = ParseFixedPointStrict(actTok[3]);
                long bx = ParseFixedPointStrict(actTok[4]);
                long by = ParseFixedPointStrict(actTok[5]);
                Assert.IsTrue(
                    VerifyScaledX(mines, n, ax, ay, bx, by),
                    $"Case #{tc + 1} geometric verification failed for {results[tc]}");
            }
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, ValidateQ3);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, ValidateQ3);
        }

        [Test]
        public void Q3Large()
        {
            SlowTest(Q3Solver, ValidateQ3);
        }

        #endregion

        #region Question 4 - Can't Stop

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n, out int d, out int k);
            test.Get(out int[] rolls);
            return () => Q4Solver(n, d, k, rolls);
        }

        private const int CantStopHashCap = 256;
        private const int CantStopHashMask = CantStopHashCap - 1;

        private static ulong Pack2(int a, int b) =>
            a < b ? ((ulong)a << 17) | (uint)b : ((ulong)b << 17) | (uint)a;

        private static ulong Pack3(int a, int b, int c)
        {
            if (a > b) (a, b) = (b, a);
            if (b > c) (b, c) = (c, b);
            if (a > b) (a, b) = (b, a);
            return ((ulong)a << 34) | ((ulong)b << 17) | (uint)c;
        }

        private static int LookupMinLeft(ulong[] keys, int[] vals, ulong key)
        {
            int slot = (int)((key * 0x9E3779B97F4A7C15UL) >> 56) & CantStopHashMask;
            while (keys[slot] != 0UL)
            {
                if (keys[slot] == key)
                    return vals[slot];
                slot = (slot + 1) & CantStopHashMask;
            }
            return int.MaxValue;
        }

        private static void UpsertMinLeft(
            ulong[] keys,
            int[] vals,
            List<int> dirtySlots,
            ulong key,
            int left)
        {
            int slot = (int)((key * 0x9E3779B97F4A7C15UL) >> 56) & CantStopHashMask;
            while (keys[slot] != 0UL)
            {
                if (keys[slot] == key)
                {
                    if (left < vals[slot])
                        vals[slot] = left;
                    return;
                }
                slot = (slot + 1) & CantStopHashMask;
            }
            keys[slot] = key;
            vals[slot] = left;
            dirtySlots.Add(slot);
        }

        private static void ResetHashTable(ulong[] keys, List<int> dirtySlots)
        {
            foreach (int slot in dirtySlots)
                keys[slot] = 0UL;
            dirtySlots.Clear();
        }

        private static string Q4Solver(int n, int d, int k, int[] rolls)
        {
            int bestL = 0;
            int bestR = 0;

            (int A, int L)[] cur1 = new (int, int)[4];
            (int A, int L)[] next1 = new (int, int)[4];
            int cur1Cnt = 0;

            (int A, int B, int L)[] cur2 = new (int, int, int)[16];
            (int A, int B, int L)[] next2 = new (int, int, int)[16];
            int cur2Cnt = 0;

            (int A, int B, int C, int L)[] cur3 = new (int, int, int, int)[64];
            (int A, int B, int C, int L)[] next3 = new (int, int, int, int)[64];
            int cur3Cnt = 0;

            ulong[] keys1 = new ulong[CantStopHashCap];
            int[] vals1 = new int[CantStopHashCap];
            List<int> dirty1 = [with(capacity: 8)];

            ulong[] keys2 = new ulong[CantStopHashCap];
            int[] vals2 = new int[CantStopHashCap];
            List<int> dirty2 = [with(capacity: 32)];

            ulong[] keys3 = new ulong[CantStopHashCap];
            int[] vals3 = new int[CantStopHashCap];
            List<int> dirty3 = [with(capacity: 128)];

            int[] uRoll = new int[4];

            for (int i = 0; i < n; i++)
            {
                int baseIdx = i * d;
                int uLen = 0;
                for (int j = 0; j < d; j++)
                {
                    int val = rolls[baseIdx + j];
                    bool dup = false;
                    for (int u = 0; u < uLen; u++)
                    {
                        if (uRoll[u] == val)
                        {
                            dup = true;
                            break;
                        }
                    }
                    if (!dup)
                        uRoll[uLen++] = val;
                }

                bool ContainsRoll(int x)
                {
                    for (int u = 0; u < uLen; u++)
                    {
                        if (uRoll[u] == x)
                            return true;
                    }
                    return false;
                }

                // Transition m = 1.
                int next1Cnt = 0;
                for (int u = 0; u < uLen; u++)
                {
                    int x = uRoll[u];
                    int left = i;
                    for (int p = 0; p < cur1Cnt; p++)
                    {
                        if (cur1[p].A == x)
                        {
                            left = cur1[p].L;
                            break;
                        }
                    }
                    next1[next1Cnt++] = (x, left);
                    UpsertMinLeft(keys1, vals1, dirty1, (ulong)x, left);
                }

                // Transition m = 2.
                for (int p = 0; p < cur2Cnt; p++)
                {
                    var (a, b, l) = cur2[p];
                    if (ContainsRoll(a) || ContainsRoll(b))
                        UpsertMinLeft(keys2, vals2, dirty2, Pack2(a, b), l);
                }
                for (int p = 0; p < cur1Cnt; p++)
                {
                    var (a, l) = cur1[p];
                    if (ContainsRoll(a))
                        continue;
                    for (int u = 0; u < uLen; u++)
                        UpsertMinLeft(keys2, vals2, dirty2, Pack2(a, uRoll[u]), l);
                }

                int next2Cnt = 0;
                foreach (int slot in dirty2)
                {
                    ulong key = keys2[slot];
                    int a = (int)(key >> 17);
                    int b = (int)(key & 0x1FFFFUL);
                    int l = vals2[slot];
                    if (LookupMinLeft(keys1, vals1, (ulong)a) <= l ||
                        LookupMinLeft(keys1, vals1, (ulong)b) <= l)
                        continue;
                    next2[next2Cnt++] = (a, b, l);
                }

                int minL = i;
                for (int p = 0; p < next1Cnt; p++)
                    minL = Math.Min(minL, next1[p].L);
                for (int p = 0; p < next2Cnt; p++)
                    minL = Math.Min(minL, next2[p].L);

                // Transition m = 3 when k == 3.
                int next3Cnt = 0;
                if (k == 3)
                {
                    for (int p = 0; p < cur3Cnt; p++)
                    {
                        var (a, b, c, l) = cur3[p];
                        if (ContainsRoll(a) || ContainsRoll(b) || ContainsRoll(c))
                            UpsertMinLeft(keys3, vals3, dirty3, Pack3(a, b, c), l);
                    }
                    for (int p = 0; p < cur2Cnt; p++)
                    {
                        var (a, b, l) = cur2[p];
                        if (ContainsRoll(a) || ContainsRoll(b))
                            continue;
                        for (int u = 0; u < uLen; u++)
                            UpsertMinLeft(keys3, vals3, dirty3, Pack3(a, b, uRoll[u]), l);
                    }

                    foreach (int slot in dirty3)
                    {
                        ulong key = keys3[slot];
                        int a = (int)(key >> 34);
                        int b = (int)((key >> 17) & 0x1FFFFUL);
                        int c = (int)(key & 0x1FFFFUL);
                        int l = vals3[slot];
                        if (LookupMinLeft(keys1, vals1, (ulong)a) <= l ||
                            LookupMinLeft(keys1, vals1, (ulong)b) <= l ||
                            LookupMinLeft(keys1, vals1, (ulong)c) <= l ||
                            LookupMinLeft(keys2, vals2, Pack2(a, b)) <= l ||
                            LookupMinLeft(keys2, vals2, Pack2(a, c)) <= l ||
                            LookupMinLeft(keys2, vals2, Pack2(b, c)) <= l)
                            continue;
                        next3[next3Cnt++] = (a, b, c, l);
                        if (l < minL)
                            minL = l;
                    }
                    ResetHashTable(keys3, dirty3);
                }

                ResetHashTable(keys2, dirty2);
                ResetHashTable(keys1, dirty1);

                (cur1, next1) = (next1, cur1);
                cur1Cnt = next1Cnt;
                (cur2, next2) = (next2, cur2);
                cur2Cnt = next2Cnt;
                (cur3, next3) = (next3, cur3);
                cur3Cnt = next3Cnt;

                if (i - minL > bestR - bestL)
                {
                    bestL = minL;
                    bestR = i;
                }
            }

            return $"{bestL} {bestR}";
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

        #region Question 5 - Let Me Tell You a Story

        private const int StoryMod = 10007;

        private static Func<int> Q5Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out int[] salaries);
            return () => Q5Solver(n, salaries);
        }

        private static int Q5Solver(int n, int[] salaries)
        {
            int[] order = new int[n];
            for (int i = 0; i < n; i++)
                order[i] = i;
            Array.Sort(order, (a, b) =>
            {
                int cmp = salaries[b].CompareTo(salaries[a]);
                return cmp != 0 ? cmp : a.CompareTo(b);
            });

            int[] invRank = new int[n];
            for (int r = 0; r < n; r++)
                invRank[order[r]] = r;

            int[] lnisEnd = new int[n];
            int maxLen = OptimalFinding.LongestIncreasingSubsequenceLengths<int>(
                invRank, lnisEnd, strictlyIncreasing: true);
            if (maxLen == n)
                return 1;

            int[] countByLen = new int[maxLen + 2];
            countByLen[1] = n % StoryMod;

            int[] prev = new int[n];
            int[] next = new int[n];
            Array.Fill(prev, 1);

            FenwickTree bit = new(n);
            for (int p = 2; p <= maxLen; p++)
            {
                bit.Clear();
                int sumP = 0;
                for (int i = 0; i < n; i++)
                {
                    int r = invRank[i];
                    int cur = 0;
                    if (lnisEnd[i] >= p && r > 0)
                        cur = (int)(bit.PrefixSum(r - 1) % StoryMod);
                    next[i] = cur;
                    sumP += cur;
                    if (sumP >= StoryMod)
                        sumP -= StoryMod;
                    if (prev[i] != 0)
                        bit.Add(r, prev[i]);
                }
                countByLen[p] = sumP;
                (prev, next) = (next, prev);
            }

            int[] fact = new int[n + 1];
            fact[0] = 1;
            for (int i = 1; i <= n; i++)
                fact[i] = (fact[i - 1] * i) % StoryMod;

            int ans = 0;
            for (int k = 1; k <= maxLen; k++)
            {
                int addTerm = (countByLen[k] * fact[n - k]) % StoryMod;
                ans += addTerm;
                if (k < n && k + 1 <= maxLen)
                {
                    int subTerm = (countByLen[k + 1] * fact[n - k - 1]) % StoryMod;
                    subTerm = (subTerm * ((k + 1) % StoryMod)) % StoryMod;
                    ans -= subTerm;
                }
                ans %= StoryMod;
            }

            return ((ans % StoryMod) + StoryMod) % StoryMod;
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
    }
}
