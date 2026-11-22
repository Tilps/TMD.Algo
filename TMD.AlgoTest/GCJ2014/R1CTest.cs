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
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2014
{
    [TestFixture]
    public class R1CTest : BaseGCJTest
    {
        #region Question 1 - Part Elf

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out string raw);
            return () => Q1Solver(raw);
        }

        private static string Q1Solver(string raw)
        {
            int slash = raw.IndexOf('/');
            long p = long.Parse(raw.AsSpan(0, slash));
            long q = long.Parse(raw.AsSpan(slash + 1));

            long g = MathFormulas.Gcd(p, q);
            p /= g;
            q /= g;

            if ((q & (q - 1L)) != 0L)
                return "impossible";

            int gen = 0;
            while (p < q)
            {
                p <<= 1;
                gen++;
            }
            return gen.ToString();
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

        #region Question 2 - Reordering Train Cars

        private const long TrainMod = 1_000_000_007L;

        private static Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out string[] cars);
            return () => Q2Solver(n, cars);
        }

        private static long Q2Solver(int n, string[] cars)
        {
            int[] single = new int[26];
            int[] mid = new int[26];
            int[] outDeg = new int[26];
            int[] inDeg = new int[26];
            int[] next = new int[26];
            Array.Fill(next, -1);
            int multiCount = 0;

            Span<int> collapsed = stackalloc int[100];
            foreach (string s in cars)
            {
                int len = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    int c = s[i] - 'a';
                    if (len == 0 || collapsed[len - 1] != c)
                        collapsed[len++] = c;
                }

                if (len == 1)
                {
                    single[collapsed[0]]++;
                }
                else
                {
                    int seenMask = 0;
                    for (int i = 0; i < len; i++)
                    {
                        int bit = 1 << collapsed[i];
                        if ((seenMask & bit) != 0)
                            return 0L;
                        seenMask |= bit;
                    }

                    for (int i = 1; i < len - 1; i++)
                    {
                        if (++mid[collapsed[i]] > 1)
                            return 0L;
                    }

                    int u = collapsed[0];
                    int v = collapsed[len - 1];
                    if (++outDeg[u] > 1 || ++inDeg[v] > 1)
                        return 0L;
                    next[u] = v;
                    multiCount++;
                }
            }

            for (int c = 0; c < 26; c++)
            {
                if (mid[c] > 0 && (outDeg[c] > 0 || inDeg[c] > 0 || single[c] > 0))
                    return 0L;
            }

            int components = 0;
            int visitedEdges = 0;
            for (int c = 0; c < 26; c++)
            {
                if (outDeg[c] == 1 && inDeg[c] == 0)
                {
                    components++;
                    int cur = c;
                    while (outDeg[cur] == 1)
                    {
                        visitedEdges++;
                        cur = next[cur];
                    }
                }
            }

            // Detect directed cycles among multi-character edges.
            if (visitedEdges != multiCount)
                return 0L;

            for (int c = 0; c < 26; c++)
            {
                if (single[c] > 0 && outDeg[c] == 0 && inDeg[c] == 0)
                    components++;
            }

            long[] fact = new long[n + 1];
            fact[0] = 1L;
            for (int i = 1; i <= n; i++)
                fact[i] = (fact[i - 1] * i) % TrainMod;

            long ways = fact[components];
            for (int c = 0; c < 26; c++)
                ways = (ways * fact[single[c]]) % TrainMod;
            return ways;
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

        #region Question 3 - Enclosure

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m, out int k);
            return () => Q3Solver(n, m, k);
        }

        private static int Tri(int s) => (s * (s + 1)) >> 1;

        private static int Q3Solver(int n, int m, int k)
        {
            if (n > m)
                (n, m) = (m, n);

            int best = k;
            for (int r = 2; r <= n; r++)
            {
                for (int c = r; c <= m; c++)
                {
                    if (r * c < k)
                        continue;

                    int maxCut = 2 * r - 1;
                    for (int i = 0; i <= maxCut; i++)
                    {
                        int cover = r * c - Tri(i / 4) - Tri((i + 1) / 4) - Tri((i + 2) / 4) - Tri((i + 3) / 4);
                        if (cover < k)
                            break;
                        int stones = 2 * (r + c) - 4 - i;
                        if (stones < best)
                            best = stones;
                    }
                }
            }
            return best;
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
