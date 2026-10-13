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
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2008
{
    [TestFixture]
    public class EmeaTest : BaseGCJTest
    {
        #region Question 1 - Scaled Triangle

        private Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int x1, out int y1, out int x2, out int y2, out int x3, out int y3);
            test.Get(out int xp1, out int yp1, out int xp2, out int yp2, out int xp3, out int yp3);

            return () => Q1Solver(
                new Complex(x1, y1), new Complex(x2, y2), new Complex(x3, y3),
                new Complex(xp1, yp1), new Complex(xp2, yp2), new Complex(xp3, yp3));
        }

        private string Q1Solver(
            Complex z1, Complex z2, Complex z3,
            Complex w1, Complex w2, Complex w3)
        {
            Complex dz = z2 - z1;
            Complex dw = w2 - w1;
            if (dz.Magnitude < 1e-9)
            {
                dz = z3 - z1;
                dw = w3 - w1;
            }

            Complex alpha = dw / dz;
            Complex beta = w1 - alpha * z1;

            Complex denom = Complex.One - alpha;
            if (denom.Magnitude < 1e-12)
                return "No Solution";

            Complex fixedPoint = beta / denom;
            return $"{fixedPoint.Real:F6} {fixedPoint.Imaginary:F6}";
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver, 1e-4);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver, 1e-4);
        }

        [Test]
        public void Q1Large()
        {
            Test(Q1Solver, 1e-4);
        }

        #endregion

        #region Question 2 - Painting a Fence

        private struct Offer
        {
            public int Color;
            public int A;
            public int B;

            public Offer(int color, int a, int b)
            {
                Color = color;
                A = a;
                B = b;
            }
        }

        private Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out (string Color, int A, int B)[] rawOffers);
            return () => Q2Solver(rawOffers);
        }

        private string Q2Solver((string Color, int A, int B)[] rawOffers)
        {
            const int TargetEnd = 10000;
            var colorMap = new Dictionary<string, int>();
            var allOffers = new List<Offer>(rawOffers.Length);

            foreach (var off in rawOffers)
            {
                if (!colorMap.TryGetValue(off.Color, out int cId))
                {
                    cId = colorMap.Count;
                    colorMap[off.Color] = cId;
                }
                allOffers.Add(new Offer(cId, off.A, off.B));
            }

            int numColors = colorMap.Count;
            var offersByColor = new List<Offer>[numColors];
            for (int i = 0; i < numColors; i++)
                offersByColor[i] = new List<Offer>();

            foreach (var offer in allOffers)
                offersByColor[offer.Color].Add(offer);

            // Filter dominated offers within the same color
            var filteredByColor = new List<Offer>[numColors];
            var startColors = new HashSet<int>();
            var endColors = new HashSet<int>();

            for (int c = 0; c < numColors; c++)
            {
                var list = offersByColor[c];
                list.Sort((x, y) => x.A != y.A ? x.A.CompareTo(y.A) : y.B.CompareTo(x.B));

                var nonDominated = new List<Offer>();
                int maxB = -1;
                foreach (var off in list)
                {
                    if (off.B > maxB)
                    {
                        nonDominated.Add(off);
                        maxB = off.B;
                        if (off.A <= 1) startColors.Add(c);
                        if (off.B >= TargetEnd) endColors.Add(c);
                    }
                }
                filteredByColor[c] = nonDominated;
            }

            if (startColors.Count == 0 || endColors.Count == 0)
                return "IMPOSSIBLE";

            int best = int.MaxValue;

            int EvaluateColors(int c1, int c2 = -1, int c3 = -1)
            {
                int covered = 0;
                int count = 0;

                while (covered < TargetEnd)
                {
                    int bestB = covered;

                    void Check(int c)
                    {
                        if (c < 0) return;
                        var list = filteredByColor[c];
                        foreach (var off in list)
                        {
                            if (off.A <= covered + 1)
                            {
                                if (off.B > bestB) bestB = off.B;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    Check(c1);
                    Check(c2);
                    Check(c3);

                    if (bestB <= covered) return -1;
                    covered = bestB;
                    count++;
                    if (count >= best) return -1;
                }
                return count;
            }

            // 1 color check
            foreach (int c in startColors)
            {
                if (endColors.Contains(c))
                {
                    int r = EvaluateColors(c);
                    if (r > 0 && r < best) best = r;
                }
            }

            if (best == 1) return "1";

            // 2 colors check
            foreach (int c1 in startColors)
            {
                foreach (int c2 in endColors)
                {
                    if (c1 == c2) continue;
                    int r = EvaluateColors(c1, c2);
                    if (r > 0 && r < best) best = r;
                }
            }

            if (best <= 2) return best.ToString();

            // 3 colors check
            var startList = startColors.ToList();
            var endList = endColors.ToList();

            // Case 3a: c1 in Start, c2 in End, c3 in any
            for (int i = 0; i < startList.Count; i++)
            {
                int c1 = startList[i];
                for (int j = 0; j < endList.Count; j++)
                {
                    int c2 = endList[j];
                    if (c1 == c2) continue;
                    for (int c3 = 0; c3 < numColors; c3++)
                    {
                        if (c3 == c1 || c3 == c2) continue;
                        int r = EvaluateColors(c1, c2, c3);
                        if (r > 0 && r < best) best = r;
                    }
                }
            }

            // Case 3b: c1, c2 in Start, c3 in End (already covered by 3a since c3 in End and c1 in Start)
            // Case 3c: c1 in Start and End, c2 and c3 any
            var both = startColors.Intersect(endColors).ToList();
            foreach (int c1 in both)
            {
                for (int c2 = 0; c2 < numColors; c2++)
                {
                    if (c2 == c1) continue;
                    for (int c3 = c2 + 1; c3 < numColors; c3++)
                    {
                        if (c3 == c1) continue;
                        int r = EvaluateColors(c1, c2, c3);
                        if (r > 0 && r < best) best = r;
                    }
                }
            }

            return best == int.MaxValue ? "IMPOSSIBLE" : best.ToString();
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

        #region Question 3 - Rainbow Trees

        private Func<long> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n, out int k);
            test.GetLines(n - 1, out (int U, int V)[] edges);
            return () => Q3Solver(n, k, edges);
        }

        private long Q3Solver(int n, int k, (int U, int V)[] edges)
        {
            var adj = new List<int>[n];
            for (int i = 0; i < n; i++)
                adj[i] = new List<int>();

            foreach (var e in edges)
            {
                adj[e.U - 1].Add(e.V - 1);
                adj[e.V - 1].Add(e.U - 1);
            }

            int root = 0;
            int[] parent = new int[n];
            parent[root] = -1;
            var bfsOrder = new List<int>(n);

            Search.Bfs(root, (u, p, depth) =>
            {
                parent[u] = p;
                bfsOrder.Add(u);
            }, u => adj[u]);

            var mod = new Modulo(1000000009);
            long ans = 1;

            // Root choices
            int rootDeg = adj[root].Count;
            for (int i = 0; i < rootDeg; i++)
            {
                long choices = k - i;
                if (choices <= 0) return 0;
                ans = mod.Multiply(ans, choices);
            }

            // Non-root choices
            for (int idx = 1; idx < bfsOrder.Count; idx++)
            {
                int u = bfsOrder[idx];
                int p = parent[u];
                int pDeg = adj[p].Count;
                int childrenCount = adj[u].Count - 1;

                for (int i = 0; i < childrenCount; i++)
                {
                    long choices = k - pDeg - i;
                    if (choices <= 0) return 0;
                    ans = mod.Multiply(ans, choices);
                }
            }

            return ans;
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

        #region Question 4 - Bus Stops

        private Func<long> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n, out int k, out int p);
            return () => Q4Solver(n, k, p);
        }

        private long Q4Solver(int n, int k, int p)
        {
            // All states are masks of length P, with bit 0 set to 1, and total number of set bits equal to K.
            // Bits represent bus offsets relative to current step: {0, offset_1, ..., offset_{K-1}} where offsets in [1, P-1].
            var states = new List<int>();
            int limit = 1 << (p - 1);
            for (int m = 0; m < limit; m++)
            {
                if (BitOperations.PopCount((uint)m) == k - 1)
                {
                    int fullMask = (m << 1) | 1;
                    states.Add(fullMask);
                }
            }

            var stateIndex = new Dictionary<int, int>(states.Count);
            for (int i = 0; i < states.Count; i++)
                stateIndex[states[i]] = i;

            int numStates = states.Count;
            long[,] matrix = new long[numStates, numStates];

            int msb = 1 << (p - 1);
            for (int i = 0; i < numStates; i++)
            {
                int mask = states[i];
                if ((mask & msb) != 0)
                {
                    // Bus at offset P-1 MUST move to current+1 (offset 0)
                    int remaining = mask ^ msb;
                    int nextMask = (remaining << 1) | 1;
                    int j = stateIndex[nextMask];
                    matrix[j, i] = (matrix[j, i] + 1) % 30031;
                }
                else
                {
                    // Any of the K buses can move to current+1 (offset 0)
                    for (int bit = 0; bit < p; bit++)
                    {
                        if ((mask & (1 << bit)) != 0)
                        {
                            int remaining = mask ^ (1 << bit);
                            int nextMask = (remaining << 1) | 1;
                            int j = stateIndex[nextMask];
                            matrix[j, i] = (matrix[j, i] + 1) % 30031;
                        }
                    }
                }
            }

            int initialMask = (1 << k) - 1;
            int initialIdx = stateIndex[initialMask];

            long power = n - k;
            var mod = new Modulo(30031);
            long[,] poweredMatrix = MatrixHelper.Power(matrix, power, mod);

            return poweredMatrix[initialIdx, initialIdx];
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
