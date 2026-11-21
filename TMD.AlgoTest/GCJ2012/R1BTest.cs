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

namespace TMD.AlgoTest.GCJ2012
{
    [TestFixture]
    public class R1BTest : BaseGCJTest
    {
        #region Question 1 - Safety in Numbers

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int[] tokens);
            return () => Q1Solver(tokens);
        }

        private static string Q1Solver(int[] tokens)
        {
            int n = tokens[0];
            int[] s = new int[n];
            int[] sorted = new int[n];
            int x = 0;
            for (int i = 0; i < n; i++)
            {
                s[i] = tokens[i + 1];
                sorted[i] = s[i];
                x += s[i];
            }

            Array.Sort(sorted);
            int pref = sorted[0];
            int k = 1;
            while (k < n && x + pref > k * sorted[k])
            {
                pref += sorted[k];
                k++;
            }

            double threshold = (double)(x + pref) / k;
            string[] ans = new string[n];
            for (int i = 0; i < n; i++)
            {
                double pct = 100.0 * Math.Max(0.0, (threshold - s[i]) / x);
                ans[i] = pct.ToString("F6", CultureInfo.InvariantCulture);
            }

            return string.Join(" ", ans);
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver, 1e-5);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver, 1e-5);
        }

        [Test]
        public void Q1Large()
        {
            Test(Q1Solver, 1e-5);
        }

        #endregion

        #region Question 2 - Tide Goes In, Tide Goes Out

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int h, out int n, out int m);
            test.GetMatrix(n, out int[][] ceiling);
            test.GetMatrix(n, out int[][] floor);
            return () => Q2Solver(h, n, m, ceiling, floor);
        }

        private static readonly int[] TideDr = [-1, 1, 0, 0];
        private static readonly int[] TideDc = [0, 0, -1, 1];

        private static string Q2Solver(int h, int n, int m, int[][] ceiling, int[][] floor)
        {
            int target = n * m - 1;

            List<KeyValuePair<int, int>> Transitions(int state, int curTime)
            {
                int r = state / m;
                int c = state % m;
                int cCur = ceiling[r][c];
                int fCur = floor[r][c];
                List<KeyValuePair<int, int>> nexts = [with(capacity: 4)];

                for (int k = 0; k < 4; k++)
                {
                    int nr = r + TideDr[k];
                    int nc = c + TideDc[k];
                    if ((uint)nr >= (uint)n || (uint)nc >= (uint)m)
                        continue;

                    int cNr = ceiling[nr][nc];
                    int fNr = floor[nr][nc];
                    if (cNr - fNr < 50 || cNr - fCur < 50 || cCur - fNr < 50)
                        continue;

                    int minDropTime = h - (cNr - 50);
                    int dep = Math.Max(curTime, minDropTime);
                    int nextTime = dep == 0
                        ? 0
                        : dep + (h - dep >= fCur + 20 ? 10 : 100);

                    nexts.Add(new KeyValuePair<int, int>(nr * m + nc, nextTime));
                }

                return nexts;
            }

            if (!Search.TryPfs(0, 0, state => state == target, Transitions, out int dist))
                throw new InvalidOperationException("Guaranteed solvable cave had no valid path.");

            return (dist / 10.0).ToString("F1", CultureInfo.InvariantCulture);
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

        #region Question 3 - Equal Sums

        private static Func<string[]> Q3Solver(GCJTestCase test)
        {
            test.Get(out long[] tokens);
            return () => Q3Solver(tokens);
        }

        private static string[] Q3Solver(long[] tokens)
        {
            int n = (int)tokens[0];
            long[] s = new long[n];
            Array.Copy(tokens, 1, s, 0, n);

            if (n <= 24)
            {
                Dictionary<long, int> seen = [with(capacity: 1 << n)];
                int limit = 1 << n;
                for (int mask = 1; mask < limit; mask++)
                {
                    long sum = 0;
                    for (int i = 0; i < n; i++)
                    {
                        if ((mask & (1 << i)) != 0)
                            sum += s[i];
                    }
                    if (seen.TryGetValue(sum, out int other))
                    {
                        int common = mask & other;
                        int m1 = mask ^ common;
                        int m2 = other ^ common;
                        List<long> s1 = [], s2 = [];
                        for (int i = 0; i < n; i++)
                        {
                            if ((m1 & (1 << i)) != 0) s1.Add(s[i]);
                            if ((m2 & (1 << i)) != 0) s2.Add(s[i]);
                        }
                        return [string.Join(" ", s1), string.Join(" ", s2)];
                    }
                    seen[sum] = mask;
                }
                return ["Impossible"];
            }

            const int TableSize = 1 << 23;
            const int TableMask = TableSize - 1;
            const int MaxInserts = 5_500_000;
            long[] tableKeys = new long[TableSize];
            ulong[] tableVals = new ulong[TableSize];
            int[] idx = new int[6];

            for (int seed = 42; ; seed++)
            {
                Array.Clear(tableKeys);
                Random rng = new(seed);
                for (int iter = 0; iter < MaxInserts; iter++)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        while (true)
                        {
                            int cand = rng.Next(n);
                            bool dup = false;
                            for (int j = 0; j < k; j++)
                            {
                                if (idx[j] == cand)
                                {
                                    dup = true;
                                    break;
                                }
                            }
                            if (!dup)
                            {
                                idx[k] = cand;
                                break;
                            }
                        }
                    }

                    // Insertion sort the 6 indices so identical sets produce identical canonical masks.
                    for (int k = 1; k < 6; k++)
                    {
                        int v = idx[k];
                        int j = k - 1;
                        while (j >= 0 && idx[j] > v)
                        {
                            idx[j + 1] = idx[j];
                            j--;
                        }
                        idx[j + 1] = v;
                    }

                    long sum = 0;
                    ulong packed = 0;
                    for (int k = 0; k < 6; k++)
                    {
                        sum += s[idx[k]];
                        packed |= (ulong)idx[k] << (k * 9);
                    }

                    int slot = (int)((ulong)sum * 11400714819323198485UL >> (64 - 23)) & TableMask;
                    while (true)
                    {
                        long existing = tableKeys[slot];
                        if (existing == 0)
                        {
                            tableKeys[slot] = sum;
                            tableVals[slot] = packed;
                            break;
                        }
                        if (existing == sum)
                        {
                            ulong other = tableVals[slot];
                            if (other != packed)
                            {
                                HashSet<int> set1 = [];
                                HashSet<int> set2 = [];
                                for (int k = 0; k < 6; k++)
                                {
                                    set1.Add((int)((packed >> (k * 9)) & 0x1FF));
                                    set2.Add((int)((other >> (k * 9)) & 0x1FF));
                                }

                                List<long> out1 = [];
                                List<long> out2 = [];
                                foreach (int id in set1)
                                {
                                    if (!set2.Contains(id))
                                        out1.Add(s[id]);
                                }
                                foreach (int id in set2)
                                {
                                    if (!set1.Contains(id))
                                        out2.Add(s[id]);
                                }
                                return [string.Join(" ", out1), string.Join(" ", out2)];
                            }
                            break;
                        }
                        slot = (slot + 1) & TableMask;
                    }
                }
            }
        }

        private static bool TryParseStrictPositiveLong(string token, out long value)
        {
            value = 0;
            if (string.IsNullOrEmpty(token))
                return false;
            if ((token.Length > 1 && token[0] == '0') || token.StartsWith("-0", StringComparison.Ordinal))
                return false;
            return long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static void ValidateQ3(string input, string expectedOutput, List<string> result)
        {
            string[] inLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            string[] expLines = expectedOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            int numCases = int.Parse(inLines[0].Trim(), CultureInfo.InvariantCulture);
            Assert.AreEqual(numCases, expLines.Length);
            Assert.AreEqual(numCases, result.Count);

            for (int caseIdx = 0; caseIdx < numCases; caseIdx++)
            {
                string[] inToks = inLines[1 + caseIdx].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int n = int.Parse(inToks[0], CultureInfo.InvariantCulture);
                HashSet<long> allowed = [with(capacity: n)];
                for (int i = 0; i < n; i++)
                    allowed.Add(long.Parse(inToks[1 + i], CultureInfo.InvariantCulture));

                bool expImpossible = expLines[caseIdx].Contains("Impossible", StringComparison.OrdinalIgnoreCase);
                string[] outLines = result[caseIdx].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual($"Case #{caseIdx + 1}:", outLines[0].Trim(), $"Case #{caseIdx + 1}: bad header.");

                if (expImpossible)
                {
                    Assert.AreEqual(2, outLines.Length);
                    Assert.AreEqual("impossible", outLines[1].Trim().ToLowerInvariant());
                    continue;
                }

                Assert.AreEqual(3, outLines.Length, $"Case #{caseIdx + 1}: expected 2 subset lines.");
                HashSet<long> ParseSubset(string line, out Int128 sum)
                {
                    string[] toks = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    Assert.IsTrue(toks.Length > 0);
                    HashSet<long> set = [];
                    sum = 0;
                    foreach (string tok in toks)
                    {
                        Assert.IsTrue(TryParseStrictPositiveLong(tok, out long val), $"Invalid integer '{tok}'");
                        Assert.IsTrue(allowed.Contains(val), $"Value {val} not in input set.");
                        Assert.IsTrue(set.Add(val), $"Duplicate value {val} in subset.");
                        sum += val;
                    }
                    return set;
                }

                HashSet<long> s1 = ParseSubset(outLines[1], out Int128 sum1);
                HashSet<long> s2 = ParseSubset(outLines[2], out Int128 sum2);
                Assert.IsFalse(s1.SetEquals(s2), $"Case #{caseIdx + 1}: subsets are identical.");
                Assert.AreEqual(sum1, sum2, $"Case #{caseIdx + 1}: subset sums differ.");
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
            Test(Q3Solver, ValidateQ3);
        }

        #endregion
    }
}
