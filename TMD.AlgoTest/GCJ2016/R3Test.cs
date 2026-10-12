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
using System.Linq;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2016
{
    [TestFixture]
    public class R3Test : BaseGCJTest
    {
        #region Question 1 - Teaching Assistant

        private Func<int> Q1Solver(GCJTestCase test)
        {
            string s;
            test.Get(out s);
            return () => Q1Solver(s);
        }

        private int Q1Solver(string s)
        {
            Stack<char> stack = new Stack<char>();
            int hits = 0;
            foreach (char c in s)
            {
                if (stack.Count > 0 && stack.Peek() == c)
                {
                    stack.Pop();
                    hits++;
                }
                else
                {
                    stack.Push(c);
                }
            }
            int misses = s.Length / 2 - hits;
            return hits * 10 + misses * 5;
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

        #region Question 2 - Forest University

        private Func<string> Q2Solver(GCJTestCase test)
        {
            int n;
            test.Get(out n);
            int[] prereqs;
            test.Get(out prereqs);
            string letters;
            test.Get(out letters);
            int m;
            test.Get(out m);
            string[] words;
            test.Get(out words);
            return () => Q2Solver(n, prereqs, letters, m, words);
        }

        private string Q2Solver(int n, int[] prereqs, string letters, int m, string[] words)
        {
            int[] parents = new int[n];
            for (int i = 0; i < n; i++)
                parents[i] = prereqs[i] == 0 ? -1 : prereqs[i] - 1;

            RootedForest forest = new RootedForest(parents);

            const int K = 8000;
            int[] wordHits = new int[m];
            Random rng = new Random(42);

            char[] hatChars = new char[n];

            for (int iter = 0; iter < K; iter++)
            {
                int[] order = forest.GenerateRandomTopologicalSort(rng);

                for (int i = 0; i < n; i++) hatChars[i] = letters[order[i]];
                string hat = new string(hatChars);

                for (int w = 0; w < m; w++)
                {
                    if (hat.Contains(words[w]))
                        wordHits[w]++;
                }
            }

            double[] result = new double[m];
            for (int w = 0; w < m; w++)
                result[w] = (double)wordHits[w] / K;

            return string.Join(" ", result.Select(r => r.ToString("F3", CultureInfo.InvariantCulture)));
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, 0.03);
        }

        [Test]
        public void Q2Small()
        {
            SlowTest(Q2Solver, 0.03);
        }

        #endregion

        #region Question 3 - Rebel Against The Empire

        private Func<double> Q3Solver(GCJTestCase test)
        {
            int n, s;
            test.Get(out n, out s);
            int[][] asteroids = new int[n][];
            for (int i = 0; i < n; i++)
                test.Get(out asteroids[i]);
            return () => Q3Solver(n, s, asteroids);
        }

        private struct PrecomputedPair
        {
            public int U;
            public int V;
            public double A;
            public double B;
            public double C;
            public double MinDist;
        }

        private double Q3Solver(int n, int s, int[][] asteroids)
        {
            int pairCount = n * (n - 1) / 2;
            var pairs = new PrecomputedPair[pairCount];
            int pIdx = 0;

            for (int i = 0; i < n; i++)
            {
                double xi = asteroids[i][0];
                double yi = asteroids[i][1];
                double zi = asteroids[i][2];
                double vxi = asteroids[i][3];
                double vyi = asteroids[i][4];
                double vzi = asteroids[i][5];

                for (int j = i + 1; j < n; j++)
                {
                    double dx = xi - asteroids[j][0];
                    double dy = yi - asteroids[j][1];
                    double dz = zi - asteroids[j][2];
                    double dvx = vxi - asteroids[j][3];
                    double dvy = vyi - asteroids[j][4];
                    double dvz = vzi - asteroids[j][5];

                    double a = dvx * dvx + dvy * dvy + dvz * dvz;
                    double b = 2.0 * (dx * dvx + dy * dvy + dz * dvz);
                    double c = dx * dx + dy * dy + dz * dz;

                    double minDist;
                    if (a > 1e-12)
                    {
                        double tVertex = -b / (2.0 * a);
                        if (tVertex > 0.0)
                        {
                            double distSq = a * tVertex * tVertex + b * tVertex + c;
                            minDist = Math.Sqrt(Math.Max(0.0, distSq));
                        }
                        else
                        {
                            minDist = Math.Sqrt(c);
                        }
                    }
                    else
                    {
                        minDist = Math.Sqrt(c);
                    }

                    pairs[pIdx++] = new PrecomputedPair
                    {
                        U = i,
                        V = j,
                        A = a,
                        B = b,
                        C = c,
                        MinDist = minDist
                    };
                }
            }

            double low = 0.0;
            double high = Math.Sqrt(
                Math.Pow(asteroids[0][0] - asteroids[1][0], 2) +
                Math.Pow(asteroids[0][1] - asteroids[1][1], 2) +
                Math.Pow(asteroids[0][2] - asteroids[1][2], 2));

            for (int iter = 0; iter < 45; iter++)
            {
                double mid = (low + high) / 2.0;
                if (CanEscape(mid, n, s, pairs))
                {
                    high = mid;
                }
                else
                {
                    low = mid;
                }
            }

            return high;
        }

        private struct EdgeInterval
        {
            public int Other;
            public double Start;
            public double End;
            public int CompIdx;
        }

        private struct Component
        {
            public double Start;
            public double End;
        }

        private bool CanEscape(double d, int n, int s, PrecomputedPair[] pairs)
        {
            double dSq = d * d;
            List<EdgeInterval>[] nodeEdges = new List<EdgeInterval>[n];
            for (int i = 0; i < n; i++) nodeEdges[i] = new List<EdgeInterval>();

            foreach (var p in pairs)
            {
                if (p.MinDist > d) continue;

                double tStart, tEnd;
                if (p.A < 1e-12)
                {
                    if (p.C <= dSq)
                    {
                        tStart = 0.0;
                        tEnd = 1e9;
                    }
                    else continue;
                }
                else
                {
                    double cPrime = p.C - dSq;
                    double disc = p.B * p.B - 4.0 * p.A * cPrime;
                    if (disc < 0.0) continue;
                    double sqrtDisc = Math.Sqrt(Math.Max(0.0, disc));
                    tStart = (-p.B - sqrtDisc) / (2.0 * p.A);
                    tEnd = (-p.B + sqrtDisc) / (2.0 * p.A);
                    if (tEnd < 0.0) continue;
                    tStart = Math.Max(0.0, tStart);
                }

                if (tStart <= tEnd)
                {
                    nodeEdges[p.U].Add(new EdgeInterval { Other = p.V, Start = tStart, End = tEnd });
                    nodeEdges[p.V].Add(new EdgeInterval { Other = p.U, Start = tStart, End = tEnd });
                }
            }

            List<Component>[] components = new List<Component>[n];
            for (int i = 0; i < n; i++) components[i] = new List<Component>();

            for (int i = 0; i < n; i++)
            {
                var edges = nodeEdges[i];
                if (edges.Count == 0) continue;

                var rawIntervals = edges.Select(e => (e.Start, e.End)).OrderBy(e => e.Start).ToList();
                var merged = new List<(double Start, double End)>();
                double curS = rawIntervals[0].Start;
                double curE = rawIntervals[0].End;

                for (int j = 1; j < rawIntervals.Count; j++)
                {
                    if (rawIntervals[j].Start <= curE)
                    {
                        curE = Math.Max(curE, rawIntervals[j].End);
                    }
                    else
                    {
                        merged.Add((curS, curE));
                        curS = rawIntervals[j].Start;
                        curE = rawIntervals[j].End;
                    }
                }
                merged.Add((curS, curE));

                double compS = merged[0].Start;
                double compE = merged[0].End;
                for (int j = 1; j < merged.Count; j++)
                {
                    if (merged[j].Start - compE <= s)
                    {
                        compE = merged[j].End;
                    }
                    else
                    {
                        components[i].Add(new Component { Start = compS, End = compE });
                        compS = merged[j].Start;
                        compE = merged[j].End;
                    }
                }
                components[i].Add(new Component { Start = compS, End = compE });

                for (int eIdx = 0; eIdx < edges.Count; eIdx++)
                {
                    var edge = edges[eIdx];
                    int cIdx = 0;
                    while (cIdx < components[i].Count && components[i][cIdx].End < edge.Start)
                        cIdx++;
                    edge.CompIdx = cIdx;
                    edges[eIdx] = edge;
                }
            }

            List<KeyValuePair<(int node, int comp), double>> initStates = new List<KeyValuePair<(int node, int comp), double>>();
            for (int c = 0; c < components[0].Count; c++)
            {
                if (components[0][c].Start <= s)
                    initStates.Add(new KeyValuePair<(int node, int comp), double>((0, c), 0.0));
            }

            return Search.TryPfs(
                initStates,
                state => state.node == 1,
                (state, tArr) =>
                {
                    List<KeyValuePair<(int node, int comp), double>> next = new List<KeyValuePair<(int node, int comp), double>>();
                    int u = state.node;
                    int compU = state.comp;

                    foreach (var edge in nodeEdges[u])
                    {
                        if (edge.CompIdx != compU) continue;
                        if (tArr > edge.End) continue;

                        double tJump = Math.Max(tArr, edge.Start);
                        int v = edge.Other;

                        if (v == 1)
                        {
                            next.Add(new KeyValuePair<(int node, int comp), double>((1, 0), tJump));
                            continue;
                        }

                        int vComp = 0;
                        while (vComp < components[v].Count && components[v][vComp].End < tJump)
                            vComp++;

                        if (vComp < components[v].Count && components[v][vComp].Start <= tJump)
                        {
                            next.Add(new KeyValuePair<(int node, int comp), double>((v, vComp), tJump));
                        }
                    }

                    return next;
                },
                out _);
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, 1e-4);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, 1e-4);
        }

        [Test]
        public void Q3Large()
        {
            SlowTest(Q3Solver, 1e-4);
        }

        #endregion

        #region Question 4 - Go++

        private Func<string> Q4Solver(GCJTestCase test)
        {
            int n, l;
            test.Get(out n, out l);
            string[] g;
            test.Get(out g);
            string b;
            test.Get(out b);
            return () => Q4Solver(n, l, g, b);
        }

        private string Q4Solver(int n, int l, string[] g, string b)
        {
            HashSet<string> goodSet = new HashSet<string>(g);
            if (goodSet.Contains(b))
                return "IMPOSSIBLE";

            StringBuilder p1 = new StringBuilder();
            foreach (char c in b)
            {
                p1.Append(c == '0' ? '1' : '0');
                p1.Append('?');
            }

            StringBuilder p2 = new StringBuilder();
            if (l == 1)
            {
                p2.Append(b[0] == '0' ? '1' : '0');
            }
            else
            {
                for (int i = 0; i < l - 1; i++)
                {
                    p2.Append(b[i] == '0' ? "10" : "01");
                }
            }

            return $"{p1} {p2}";
        }

        private void ValidateGo(string input, string expectedOutput, List<string> result)
        {
            string[] inLines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int t = int.Parse(inLines[0]);
            int inIdx = 1;

            List<string> resLines = new List<string>();
            foreach (var r in result)
                resLines.AddRange(r.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

            int resIdx = 0;
            for (int caseNum = 1; caseNum <= t; caseNum++)
            {
                string[] nl = inLines[inIdx++].Split(' ');
                int n = int.Parse(nl[0]);
                int l = int.Parse(nl[1]);
                string[] g = inLines[inIdx++].Split(' ');
                string b = inLines[inIdx++];

                string actualLine = resLines[resIdx++];
                string header = $"Case #{caseNum}:";
                Assert.IsTrue(actualLine.StartsWith(header), $"Expected header {header}");
                string actualContent = actualLine.Substring(header.Length).Trim();

                bool expectedImpossible = g.Contains(b);
                if (expectedImpossible)
                {
                    Assert.AreEqual("IMPOSSIBLE", actualContent);
                    continue;
                }

                Assert.AreNotEqual("IMPOSSIBLE", actualContent);
                string[] parts = actualContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(2, parts.Length, "Expected two programs");
                string p = parts[0];
                string q = parts[1];

                Assert.IsTrue(p.Length >= 1 && q.Length >= 1, "Programs must be non-empty");
                Assert.IsTrue(p.Length + q.Length <= 200, "Combined program length <= 200");
                Assert.AreEqual(l, p.Count(c => c == '?') + q.Count(c => c == '?'), "Total '?' count must equal L");

                bool[] canGen = CanGenerate(p, q, new[] { b }.Concat(g).ToArray(), l);
                Assert.IsFalse(canGen[0], $"Programs can generate bad string {b}");
                for (int i = 0; i < n; i++)
                {
                    Assert.IsTrue(canGen[i + 1], $"Programs cannot generate good string {g[i]}");
                }
            }
        }

        private static bool[] CanGenerate(string p, string q, string[] candidates, int l)
        {
            int a = p.Length;
            int b = q.Length;
            int c = l;

            int[,] k = new int[a + 1, b + 1];
            for (int i = 0; i < a; i++)
                k[i + 1, 0] = k[i, 0] + (p[i] == '?' ? 1 : 0);
            for (int i = 0; i <= a; i++)
                for (int j = 0; j < b; j++)
                    k[i, j + 1] = k[i, j] + (q[j] == '?' ? 1 : 0);

            if (k[a, b] != c) return new bool[candidates.Length];

            int numCand = candidates.Length;
            BigInteger[] zeroMask = new BigInteger[c];
            BigInteger[] oneMask = new BigInteger[c];

            for (int i = 0; i < numCand; i++)
            {
                BigInteger bit = BigInteger.One << i;
                for (int pos = 0; pos < c; pos++)
                {
                    if (candidates[i][pos] == '1')
                        oneMask[pos] |= bit;
                    else
                        zeroMask[pos] |= bit;
                }
            }

            BigInteger[,] r0 = new BigInteger[a + 1, b + 1];
            BigInteger[,] r1 = new BigInteger[a + 1, b + 1];
            r0[0, 0] = (BigInteger.One << numCand) - 1;

            for (int i = 0; i <= a; i++)
            {
                for (int j = 0; j <= b; j++)
                {
                    if (i < a)
                    {
                        if (p[i] == '?')
                        {
                            r0[i + 1, j] |= r0[i, j] & zeroMask[k[i, j]];
                            r1[i + 1, j] |= r1[i, j] & oneMask[k[i, j]];
                        }
                        else
                        {
                            BigInteger any = r0[i, j] | r1[i, j];
                            if (p[i] == '0') r0[i + 1, j] |= any;
                            else r1[i + 1, j] |= any;
                        }
                    }
                    if (j < b)
                    {
                        if (q[j] == '?')
                        {
                            r0[i, j + 1] |= r0[i, j] & zeroMask[k[i, j]];
                            r1[i, j + 1] |= r1[i, j] & oneMask[k[i, j]];
                        }
                        else
                        {
                            BigInteger any = r0[i, j] | r1[i, j];
                            if (q[j] == '0') r0[i, j + 1] |= any;
                            else r1[i, j + 1] |= any;
                        }
                    }
                }
            }

            BigInteger resMask = r0[a, b] | r1[a, b];
            bool[] result = new bool[numCand];
            for (int i = 0; i < numCand; i++)
                result[i] = !((resMask & (BigInteger.One << i)).IsZero);
            return result;
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver, ValidateGo);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver, ValidateGo);
        }

        [Test]
        public void Q4Large()
        {
            SlowTest(Q4Solver, ValidateGo);
        }

        #endregion
    }
}

