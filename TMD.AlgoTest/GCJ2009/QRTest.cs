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
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2009
{
    [TestFixture]
    public class QRTest : BaseGCJTest
    {
        #region Question 1 - Alien Language

        private static (int TestCount, Func<GCJTestCase, Func<int>> TestCaseHandler) Q1Setup(GCJTestCase header)
        {
            header.Get(out int l, out int d, out int n);
            header.GetLines(d, out string[] words);
            Trie<char> trie = Trie.FromWords(words);
            return (n, test =>
            {
                test.Get(out string pattern);
                return () => Q1Solver(l, trie, pattern);
            });
        }

        private static int Q1Solver(int l, Trie<char> trie, string pattern)
        {
            return trie.CountMatches(ParsePattern(l, pattern));
        }

        private static Func<char, bool>[] ParsePattern(int l, string pattern)
        {
            var tokenPredicates = new Func<char, bool>[l];
            int pos = 0;
            for (int token = 0; token < l; token++)
            {
                if (pattern[pos] == '(')
                {
                    pos++;
                    int mask = 0;
                    while (pattern[pos] != ')')
                    {
                        mask |= 1 << (pattern[pos] - 'a');
                        pos++;
                    }
                    pos++;
                    int capturedMask = mask;
                    tokenPredicates[token] = c => c >= 'a' && c <= 'z' && (capturedMask & (1 << (c - 'a'))) != 0;
                }
                else
                {
                    char expected = pattern[pos];
                    pos++;
                    tokenPredicates[token] = c => c == expected;
                }
            }
            return tokenPredicates;
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Setup);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Setup);
        }

        [Test]
        public void Q1Large()
        {
            SlowTest(Q1Setup);
        }

        #endregion

        #region Question 2 - Watersheds

        private static readonly (int Dr, int Dc)[] FlowDirections =
        [
            (-1, 0), // North
            (0, -1), // West
            (0, 1),  // East
            (1, 0)   // South
        ];

        private static Func<string[]> Q2Solver(GCJTestCase test)
        {
            test.Get(out int h, out int w);
            test.GetMatrix(h, out int[][] altitudes);
            return () => Q2Solver(h, w, altitudes);
        }

        private static string[] Q2Solver(int h, int w, int[][] altitudes)
        {
            var basins = new MappedDisjointTracker<int>(id => id, 0, h * w - 1);
            for (int id = 0; id < h * w; id++)
                basins.Add(id);

            for (int r = 0; r < h; r++)
            {
                for (int c = 0; c < w; c++)
                {
                    int currentAlt = altitudes[r][c];
                    int bestAlt = currentAlt;
                    int bestR = -1;
                    int bestC = -1;

                    foreach (var (dr, dc) in FlowDirections)
                    {
                        int nr = r + dr;
                        int nc = c + dc;
                        if (nr < 0 || nr >= h || nc < 0 || nc >= w)
                            continue;

                        if (altitudes[nr][nc] < bestAlt)
                        {
                            bestAlt = altitudes[nr][nc];
                            bestR = nr;
                            bestC = nc;
                        }
                    }

                    if (bestR >= 0)
                    {
                        basins.Union(r * w + c, bestR * w + bestC);
                    }
                }
            }

            Dictionary<int, char> sinkLabels = [];
            char nextLabel = 'a';
            string[] rows = new string[h];
            char[] rowChars = new char[w];

            for (int r = 0; r < h; r++)
            {
                for (int c = 0; c < w; c++)
                {
                    int rep = basins.GetRepresentative(r * w + c);
                    rowChars[c] = sinkLabels.GetOrPopulate(rep, () => nextLabel++);
                }
                rows[r] = string.Join(' ', rowChars);
            }

            return rows;
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

        #region Question 3 - Welcome to Code Jam

        private const string TargetPhrase = "welcome to code jam";
        private static readonly Modulo ModQ3 = new(10_000);

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out string text);
            return () => Q3Solver(text);
        }

        private static string Q3Solver(string text)
        {
            long[] dp = new long[TargetPhrase.Length + 1];
            dp[0] = 1;

            foreach (char ch in text)
            {
                for (int j = TargetPhrase.Length - 1; j >= 0; j--)
                {
                    if (TargetPhrase[j] == ch)
                    {
                        dp[j + 1] = ModQ3.Add(dp[j + 1], dp[j]);
                    }
                }
            }

            return $"{dp[TargetPhrase.Length]:D4}";
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
