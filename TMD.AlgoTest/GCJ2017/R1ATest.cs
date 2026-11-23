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

namespace TMD.AlgoTest.GCJ2017
{
    [TestFixture]
    public class R1ATest : BaseGCJTest
    {
        #region Question 1 - Alphabet Cake

        private static Func<string[]> Q1Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c);
            test.GetLines(r, out string[] rows);
            return () => Q1Solver(r, c, rows);
        }

        private static string[] Q1Solver(int r, int c, string[] rows)
        {
            char[][] grid = new char[r][];
            int firstFilled = -1;
            for (int i = 0; i < r; i++)
            {
                char[] row = rows[i].ToCharArray();
                char last = '?';
                for (int j = 0; j < c; j++)
                {
                    if (row[j] != '?')
                        last = row[j];
                    else if (last != '?')
                        row[j] = last;
                }

                if (last != '?')
                {
                    if (firstFilled == -1)
                        firstFilled = i;
                    char next = '?';
                    for (int j = c - 1; j >= 0; j--)
                    {
                        if (row[j] != '?')
                            next = row[j];
                        else
                            row[j] = next;
                    }
                }
                grid[i] = row;
            }

            for (int i = firstFilled - 1; i >= 0; i--)
                Array.Copy(grid[firstFilled], grid[i], c);
            for (int i = firstFilled + 1; i < r; i++)
            {
                if (grid[i][0] == '?')
                    Array.Copy(grid[i - 1], grid[i], c);
            }

            string[] output = new string[r];
            for (int i = 0; i < r; i++)
                output[i] = new string(grid[i]);
            return output;
        }

        private static void ValidateAlphabetCake(string input, string expectedOutput, List<string> result)
        {
            char[] sep = ['\r', '\n'];
            string[] inLines = input.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            int t = int.Parse(inLines[0], CultureInfo.InvariantCulture);
            int inIdx = 1;
            Assert.AreEqual(t, result.Count);

            for (int caseNum = 1; caseNum <= t; caseNum++)
            {
                string[] rc = inLines[inIdx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int r = int.Parse(rc[0], CultureInfo.InvariantCulture);
                int c = int.Parse(rc[1], CultureInfo.InvariantCulture);

                string[] orig = new string[r];
                Dictionary<char, (int R, int C)> initialPos = [];
                for (int i = 0; i < r; i++)
                {
                    orig[i] = inLines[inIdx++];
                    for (int j = 0; j < c; j++)
                    {
                        if (orig[i][j] != '?')
                            initialPos[orig[i][j]] = (i, j);
                    }
                }

                string[] lines = result[caseNum - 1].Split(sep, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual($"Case #{caseNum}:", lines[0].TrimEnd());
                Assert.AreEqual(r + 1, lines.Length, $"Case #{caseNum} wrong row count");

                string[] outGrid = new string[r];
                int[] minR = new int[26];
                int[] maxR = new int[26];
                int[] minC = new int[26];
                int[] maxC = new int[26];
                int[] counts = new int[26];
                Array.Fill(minR, int.MaxValue);
                Array.Fill(minC, int.MaxValue);

                for (int i = 0; i < r; i++)
                {
                    outGrid[i] = lines[i + 1];
                    Assert.AreEqual(c, outGrid[i].Length, $"Case #{caseNum} wrong col count");
                    for (int j = 0; j < c; j++)
                    {
                        char ch = outGrid[i][j];
                        Assert.IsTrue(initialPos.ContainsKey(ch), $"Case #{caseNum} illegal char '{ch}'");
                        if (orig[i][j] != '?')
                            Assert.AreEqual(orig[i][j], ch, $"Case #{caseNum} altered fixed letter at ({i},{j})");

                        int idx = ch - 'A';
                        if (i < minR[idx]) minR[idx] = i;
                        if (i > maxR[idx]) maxR[idx] = i;
                        if (j < minC[idx]) minC[idx] = j;
                        if (j > maxC[idx]) maxC[idx] = j;
                        counts[idx]++;
                    }
                }

                for (int idx = 0; idx < 26; idx++)
                {
                    if (counts[idx] == 0)
                        continue;
                    int area = (maxR[idx] - minR[idx] + 1) * (maxC[idx] - minC[idx] + 1);
                    Assert.AreEqual(area, counts[idx], $"Case #{caseNum} letter {(char)('A' + idx)} not single rectangle");
                }
            }
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver, ValidateAlphabetCake);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver, ValidateAlphabetCake);
        }

        [Test]
        public void Q1Large()
        {
            Test(Q1Solver, ValidateAlphabetCake);
        }

        #endregion

        #region Question 2 - Ratatouille

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out int p);
            test.Get(out long[] req);
            test.GetMatrix(n, out long[][] qty);
            return () => Q2Solver(n, p, req, qty);
        }

        private static int Q2Solver(int n, int p, long[] req, long[][] qty)
        {
            List<(long Lo, long Hi)>[] intervals = new List<(long Lo, long Hi)>[n];
            for (int i = 0; i < n; i++)
            {
                List<(long Lo, long Hi)> list = new(p);
                long r = req[i];
                for (int j = 0; j < p; j++)
                {
                    long q = qty[i][j];
                    long lo = Math.Max(1L, (10 * q + 11 * r - 1) / (11 * r));
                    long hi = (10 * q) / (9 * r);
                    if (lo <= hi)
                        list.Add((lo, hi));
                }
                list.Sort(static (a, b) =>
                {
                    int cmp = a.Lo.CompareTo(b.Lo);
                    return cmp != 0 ? cmp : a.Hi.CompareTo(b.Hi);
                });
                intervals[i] = list;
            }

            int[] idx = new int[n];
            int kits = 0;
            while (true)
            {
                long maxLo = 0;
                long minHi = long.MaxValue;
                int minHiIng = -1;

                for (int i = 0; i < n; i++)
                {
                    if (idx[i] >= intervals[i].Count)
                        return kits;
                    var (lo, hi) = intervals[i][idx[i]];
                    if (lo > maxLo) maxLo = lo;
                    if (hi < minHi)
                    {
                        minHi = hi;
                        minHiIng = i;
                    }
                }

                if (maxLo <= minHi)
                {
                    kits++;
                    for (int i = 0; i < n; i++)
                        idx[i]++;
                }
                else
                {
                    idx[minHiIng]++;
                }
            }
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

        #region Question 3 - Play the Dragon

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out long hd, out long ad, out long hk, out long ak, out long b, out long d);
            return () => Q3Solver(hd, ad, hk, ak, b, d);
        }

        private static string Q3Solver(long hd, long ad, long hk, long ak, long b, long d)
        {
            // Find M = min_{k >= 0} (k + ceil(hk / (ad + k * b))).
            long m = (hk + ad - 1) / ad;
            if (b > 0)
            {
                long kLimit = Math.Min(m, (long)Math.Sqrt((double)hk / b) + 2000L);
                for (long k = 1; k <= kLimit; k++)
                {
                    Int128 atk = ad + (Int128)k * b;
                    long hits = (long)((hk + atk - 1) / atk);
                    long cand = k + hits;
                    if (cand < m)
                        m = cand;
                }
            }

            if (m == 1)
                return "1";

            long EvalBuffAttack(long curH, long curAk)
            {
                if (curH <= 0)
                    return long.MaxValue;
                if (curAk == 0)
                    return m;
                if (curH > (Int128)(m - 1) * curAk)
                    return m;

                long firstBatch = (curH - 1) / curAk;
                long rem = m - firstBatch; // >= 2
                if (curAk >= hd)
                    return long.MaxValue;

                long hAfter = hd - curAk;
                long batch = (hAfter - 1) / curAk;
                if (batch <= 0)
                    return long.MaxValue;

                long cures = (rem - 2) / batch + 1;
                return m + cures;
            }

            long best = EvalBuffAttack(hd, ak);
            if (d == 0 || ak == 0)
                return best == long.MaxValue ? "IMPOSSIBLE" : best.ToString(CultureInfo.InvariantCulture);

            long curH = hd;
            long curAk = ak;
            long turnsUsed = 0;

            static Int128 DebuffDamage(long k, long startAk, long dec) =>
                (Int128)k * startAk - (Int128)dec * k * (k + 1) / 2;

            static long SafeDebuffs(long hp, long startAk, long dec, long maxSteps)
            {
                if (maxSteps <= 0)
                    return 0;
                long maxPos = Math.Min(maxSteps, (startAk - 1) / dec);
                long lo = 0;
                long hi = maxPos;
                while (lo < hi)
                {
                    long mid = lo + ((hi - lo + 1) >> 1);
                    if (DebuffDamage(mid, startAk, dec) < hp)
                        lo = mid;
                    else
                        hi = mid - 1;
                }
                if (lo == maxPos && maxSteps > maxPos)
                    return maxSteps;
                return lo;
            }

            static void ApplySomeDebuffs(ref long hp, ref long attack, ref long turns, long dec, long count)
            {
                if (count <= 0)
                    return;
                long posCount = Math.Min(count, (attack - 1) / dec);
                hp -= (long)DebuffDamage(posCount, attack, dec);
                long nextAttack = attack - count * dec;
                attack = nextAttack > 0 ? nextAttack : 0;
                turns += count;
            }

            bool AdvanceDebuffs(long steps)
            {
                while (steps > 0)
                {
                    if (curAk == 0)
                    {
                        turnsUsed += steps;
                        return true;
                    }

                    long t = SafeDebuffs(curH, curAk, d, steps);
                    if (t > 0)
                    {
                        ApplySomeDebuffs(ref curH, ref curAk, ref turnsUsed, d, t);
                        steps -= t;
                        continue;
                    }

                    // Must cure before making another debuff.
                    long nextH = hd - curAk;
                    if (nextH <= 0)
                        return false;
                    curH = nextH;
                    turnsUsed++;

                    long cycleT = SafeDebuffs(curH, curAk, d, steps);
                    if (cycleT == 0)
                        return false;
                    if (cycleT == steps)
                    {
                        ApplySomeDebuffs(ref curH, ref curAk, ref turnsUsed, d, steps);
                        return true;
                    }

                    // Cycle of length cycleT (< kZero): check how many consecutive cycles take the exact same length cycleT.
                    Int128 denom = cycleT + 2;
                    Int128 numer = hd + (Int128)d * (cycleT + 1) * (cycleT + 2) / 2;
                    Int128 aMin = (numer + denom - 1) / denom;
                    Int128 posReq = (Int128)(cycleT + 1) * d + 1;
                    Int128 limitA = aMin > posReq ? aMin : posReq;

                    long cyclePeriod = cycleT * d;
                    long maxByA = curAk >= limitA ? (long)((curAk - limitA) / cyclePeriod) + 1 : 1;
                    long cCount = Math.Min(steps / cycleT, maxByA);

                    long lastStartAk = curAk - (cCount - 1) * cyclePeriod;
                    long dmgLast = (long)(lastStartAk + DebuffDamage(cycleT, lastStartAk, d));
                    curH = hd - dmgLast;
                    curAk -= cCount * cyclePeriod;
                    turnsUsed += cCount * cycleT + (cCount - 1);
                    steps -= cCount * cycleT;
                }
                return true;
            }

            while (curAk > 0)
            {
                long q = (hd - 1) / curAk;
                long maxA = (q + 1 >= hd) ? 0 : (hd - 1) / (q + 1);
                long steps = (curAk - maxA + d - 1) / d;
                if (steps < 1) steps = 1;

                if (!AdvanceDebuffs(steps))
                    break;

                long ba = EvalBuffAttack(curH, curAk);
                if (ba != long.MaxValue)
                {
                    long cand = turnsUsed + ba;
                    if (cand < best)
                        best = cand;
                }
            }

            return best == long.MaxValue ? "IMPOSSIBLE" : best.ToString(CultureInfo.InvariantCulture);
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
