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
using System.Text;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2013
{
    [TestFixture]
    public class R1ATest : BaseGCJTest
    {
        #region Question 1 - Bullseye

        private static Func<long> Q1Solver(GCJTestCase test)
        {
            test.Get(out long r, out long t);
            return () => Q1Solver(r, t);
        }

        private static long Q1Solver(long r, long t)
        {
            long lo = 1;
            long hi = Math.Min(t / (2 * r + 1), 1_000_000_001L);
            Int128 bigT = t;
            Int128 bigR2Minus1 = (Int128)2 * r - 1;
            long best = 1;

            while (lo <= hi)
            {
                long mid = lo + ((hi - lo) >> 1);
                Int128 needed = (Int128)mid * (bigR2Minus1 + (Int128)2 * mid);
                if (needed <= bigT)
                {
                    best = mid;
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
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
            Test(Q1Solver);
        }

        #endregion

        #region Question 2 - Manage Your Energy

        private static Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out long e, out long r, out int n);
            test.Get(out long[] v);
            return () => Q2Solver(e, r, n, v);
        }

        private static long Q2Solver(long e, long r, int n, long[] v)
        {
            if (r > e)
                r = e;

            int[] nextGreater = new int[n];
            Array.Fill(nextGreater, -1);
            int[] stack = new int[n];
            int top = 0;
            for (int i = 0; i < n; i++)
            {
                while (top > 0 && v[stack[top - 1]] < v[i])
                    nextGreater[stack[--top]] = i;
                stack[top++] = i;
            }

            long totalGain = 0;
            long cur = e;
            for (int i = 0; i < n; i++)
            {
                long spend;
                int nextIdx = nextGreater[i];
                if (nextIdx == -1)
                {
                    spend = cur;
                }
                else
                {
                    long keep = e - (long)(nextIdx - i) * r;
                    if (keep < 0)
                        keep = 0;
                    spend = cur - keep;
                    if (spend < 0)
                        spend = 0;
                }

                totalGain += spend * v[i];
                cur = cur - spend + r;
                if (cur > e)
                    cur = e;
            }
            return totalGain;
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

        #region Question 3 - Good Luck

        private readonly struct CardMultiset
        {
            public readonly string Digits;
            public readonly int C2;
            public readonly int C3;
            public readonly int C4;
            public readonly int C5;
            public readonly int C6;
            public readonly int C7;
            public readonly int C8;
            public readonly double Prior;

            public CardMultiset(string digits, int[] counts, double prior)
            {
                Digits = digits;
                C2 = counts[2];
                C3 = counts[3];
                C4 = counts[4];
                C5 = counts[5];
                C6 = counts[6];
                C7 = counts[7];
                C8 = counts[8];
                Prior = prior;
            }
        }

        private readonly struct FactorizedProduct
        {
            public readonly int E2;
            public readonly int E3;
            public readonly int E5;
            public readonly int E7;
            public readonly bool Valid;

            public FactorizedProduct(long p)
            {
                int e2 = 0, e3 = 0, e5 = 0, e7 = 0;
                while ((p & 1L) == 0) { p >>= 1; e2++; }
                while (p % 3 == 0) { p /= 3; e3++; }
                while (p % 5 == 0) { p /= 5; e5++; }
                while (p % 7 == 0) { p /= 7; e7++; }
                E2 = e2;
                E3 = e3;
                E5 = e5;
                E7 = e7;
                Valid = p == 1;
            }
        }

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int r, out int n, out int m, out int k);
            long[][] sets = new long[r][];
            for (int i = 0; i < r; i++)
                test.Get(out sets[i]);
            return () => Q3Solver(r, n, m, k, sets);
        }

        private static string Q3Solver(int r, int n, int m, int k, long[][] sets)
        {
            double[,] comb = new double[n + 1, n + 1];
            double[] fact = new double[n + 1];
            fact[0] = 1.0;
            for (int i = 1; i <= n; i++)
                fact[i] = fact[i - 1] * i;
            for (int i = 0; i <= n; i++)
            {
                comb[i, 0] = 1.0;
                comb[i, i] = 1.0;
                for (int j = 1; j < i; j++)
                    comb[i, j] = comb[i - 1, j - 1] + comb[i - 1, j];
            }

            // ways2[c2, c4, c8, rem2]: subsets of c2 2s, c4 4s, c8 8s with 2-exponent sum == rem2.
            int maxE2 = 3 * n;
            double[,,,] ways2 = new double[n + 1, n + 1, n + 1, maxE2 + 1];
            for (int c2 = 0; c2 <= n; c2++)
            {
                for (int c4 = 0; c2 + c4 <= n; c4++)
                {
                    for (int c8 = 0; c2 + c4 + c8 <= n; c8++)
                    {
                        for (int k4 = 0; k4 <= c4; k4++)
                        {
                            for (int k8 = 0; k8 <= c8; k8++)
                            {
                                double w48 = comb[c4, k4] * comb[c8, k8];
                                int base2 = 2 * k4 + 3 * k8;
                                for (int k2 = 0; k2 <= c2; k2++)
                                    ways2[c2, c4, c8, base2 + k2] += w48 * comb[c2, k2];
                            }
                        }
                    }
                }
            }

            List<CardMultiset> multisets = [];
            int[] counts = new int[9];
            char[] curDigits = new char[n];

            void Enumerate(int pos, int minDigit)
            {
                if (pos == n)
                {
                    double prior = fact[n];
                    for (int d = 2; d <= m; d++)
                        prior /= fact[counts[d]];
                    multisets.Add(new CardMultiset(new string(curDigits), counts, prior));
                    return;
                }
                for (int d = minDigit; d <= m; d++)
                {
                    counts[d]++;
                    curDigits[pos] = (char)('0' + d);
                    Enumerate(pos + 1, d);
                    counts[d]--;
                }
            }

            Enumerate(0, 2);

            StringBuilder sb = new();
            FactorizedProduct[] fp = new FactorizedProduct[k];
            for (int row = 0; row < r; row++)
            {
                int needE2 = 0, needE3 = 0, needE5 = 0, needE7 = 0;
                bool allValid = true;
                for (int j = 0; j < k; j++)
                {
                    fp[j] = new FactorizedProduct(sets[row][j]);
                    if (!fp[j].Valid)
                        allValid = false;
                    if (fp[j].E2 > needE2) needE2 = fp[j].E2;
                    if (fp[j].E3 > needE3) needE3 = fp[j].E3;
                    if (fp[j].E5 > needE5) needE5 = fp[j].E5;
                    if (fp[j].E7 > needE7) needE7 = fp[j].E7;
                }

                string bestGuess = multisets[0].Digits;
                double bestScore = -1.0;

                if (allValid)
                {
                    foreach (CardMultiset ms in multisets)
                    {
                        if (ms.C5 < needE5 || ms.C7 < needE7 || ms.C3 + ms.C6 < needE3)
                            continue;
                        if (ms.C2 + 2 * ms.C4 + ms.C6 + 3 * ms.C8 < needE2)
                            continue;

                        double score = ms.Prior;
                        for (int j = 0; j < k; j++)
                        {
                            FactorizedProduct f = fp[j];
                            double w = comb[ms.C5, f.E5] * comb[ms.C7, f.E7];
                            if (w == 0.0)
                            {
                                score = 0.0;
                                break;
                            }

                            double sum6 = 0.0;
                            int k6Min = Math.Max(0, f.E3 - ms.C3);
                            int k6Max = Math.Min(ms.C6, Math.Min(f.E3, f.E2));
                            for (int k6 = k6Min; k6 <= k6Max; k6++)
                            {
                                int rem2 = f.E2 - k6;
                                sum6 += comb[ms.C6, k6] * comb[ms.C3, f.E3 - k6] * ways2[ms.C2, ms.C4, ms.C8, rem2];
                            }
                            if (sum6 == 0.0)
                            {
                                score = 0.0;
                                break;
                            }
                            score *= w * sum6;
                        }

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestGuess = ms.Digits;
                        }
                    }
                }

                sb.AppendLine();
                sb.Append(bestGuess);
            }

            return sb.ToString();
        }

        private static readonly char[] LineSeps = ['\r', '\n'];
        private static readonly char[] TokenSeps = [' ', '\t'];

        private static void ValidateQ3(string input, string expectedOutput, List<string> results)
        {
            Assert.AreEqual(1, results.Count);
            string[] inLines = input.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            string[] header = inLines[1].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
            int r = int.Parse(header[0]);
            int n = int.Parse(header[1]);
            int m = int.Parse(header[2]);

            string[] expLines = expectedOutput.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            string[] actLines = results[0].Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(r + 1, expLines.Length);
            Assert.AreEqual(r + 1, actLines.Length);
            Assert.AreEqual("Case #1:", actLines[0].TrimEnd());

            char maxChar = (char)('0' + m);
            int correct = 0;
            for (int i = 1; i <= r; i++)
            {
                string guess = actLines[i].Trim();
                string truth = expLines[i].Trim();
                Assert.AreEqual(n, guess.Length);
                foreach (char ch in guess)
                    Assert.IsTrue(ch >= '2' && ch <= maxChar, $"Invalid digit '{ch}' on line {i}");

                char[] gArr = guess.ToCharArray();
                char[] tArr = truth.ToCharArray();
                Array.Sort(gArr);
                Array.Sort(tArr);
                if (gArr.AsSpan().SequenceEqual(tArr))
                    correct++;
            }

            int required = r == 2 ? 1 : r == 100 ? 50 : 1120;
            Assert.IsTrue(correct >= required, $"Got {correct} correct out of {r}, required {required}.");
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
    }
}
