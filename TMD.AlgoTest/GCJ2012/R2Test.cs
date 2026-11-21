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

namespace TMD.AlgoTest.GCJ2012
{
    [TestFixture]
    public class R2Test : BaseGCJTest
    {
        #region Question 1 - Swinging Wild

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out (int D, int L)[] vines);
            test.Get(out int target);
            return () => Q1Solver(vines, target);
        }

        private static string Q1Solver((int D, int L)[] vines, int target)
        {
            int n = vines.Length;
            long[] reach = new long[n];
            reach[0] = vines[0].D;
            int next = 1;

            for (int i = 0; i < n; i++)
            {
                if (i >= next)
                    return "NO";
                long maxDist = vines[i].D + reach[i];
                if (maxDist >= target)
                    return "YES";
                while (next < n && vines[next].D <= maxDist)
                {
                    reach[next] = Math.Min((long)vines[next].D - vines[i].D, vines[next].L);
                    next++;
                }
            }

            return "NO";
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

        #region Question 2 - Aerobics

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out long w, out long l);
            test.Get(out long[] r);
            return () => Q2Solver(n, w, l, r);
        }

        private static string Q2Solver(int n, long w, long l, long[] r)
        {
            int[] order = new int[n];
            for (int i = 0; i < n; i++)
                order[i] = i;
            Array.Sort(order, (a, b) => r[b].CompareTo(r[a]));

            long[] ansX = new long[n];
            long[] ansY = new long[n];
            List<(long Start, long End, long Y)> sky = [(0L, w, 0L)];

            foreach (int idx in order)
            {
                int bestSeg = 0;
                for (int k = 1; k < sky.Count; k++)
                {
                    if (sky[k].Y < sky[bestSeg].Y)
                        bestSeg = k;
                }

                long x = sky[bestSeg].Start;
                long y = sky[bestSeg].Y;
                ansX[idx] = x;
                ansY[idx] = y;

                long rad = r[idx];
                long left = Math.Max(0L, x - 2 * rad);
                long right = Math.Min(w, x + 2 * rad);
                long raised = y + 2 * rad;

                List<(long Start, long End, long Y)> nextSky = [with(capacity: sky.Count + 2)];
                void AddSeg(long s, long e, long h)
                {
                    if (s >= e)
                        return;
                    if (nextSky.Count > 0 && nextSky[^1].Y == h && nextSky[^1].End == s)
                    {
                        var prev = nextSky[^1];
                        nextSky[^1] = (prev.Start, e, h);
                    }
                    else
                    {
                        nextSky.Add((s, e, h));
                    }
                }

                foreach (var (s, e, h) in sky)
                {
                    if (e <= left || s >= right)
                    {
                        AddSeg(s, e, h);
                    }
                    else
                    {
                        if (s < left)
                            AddSeg(s, left, h);
                        long ol = Math.Max(s, left);
                        long or_ = Math.Min(e, right);
                        AddSeg(ol, or_, Math.Max(h, raised));
                        if (e > right)
                            AddSeg(right, e, h);
                    }
                }
                sky = nextSky;
            }

            string[] coords = new string[2 * n];
            for (int i = 0; i < n; i++)
            {
                coords[2 * i] = ansX[i].ToString(CultureInfo.InvariantCulture);
                coords[2 * i + 1] = ansY[i].ToString(CultureInfo.InvariantCulture);
            }
            return string.Join(" ", coords);
        }

        private static void ValidateQ2(string input, string _, List<string> result)
        {
            string[] inLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            int numCases = int.Parse(inLines[0].Trim(), CultureInfo.InvariantCulture);
            Assert.AreEqual(numCases, result.Count);

            for (int caseIdx = 0; caseIdx < numCases; caseIdx++)
            {
                string[] header = inLines[2 * caseIdx + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int n = int.Parse(header[0], CultureInfo.InvariantCulture);
                double w = double.Parse(header[1], CultureInfo.InvariantCulture);
                double l = double.Parse(header[2], CultureInfo.InvariantCulture);
                string[] radToks = inLines[2 * caseIdx + 2].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                double[] radii = Array.ConvertAll(radToks, s => double.Parse(s, CultureInfo.InvariantCulture));

                string[] outToks = result[caseIdx].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(2 + 2 * n, outToks.Length, $"Case #{caseIdx + 1}: wrong token count.");
                Assert.AreEqual("Case", outToks[0]);
                Assert.AreEqual($"#{caseIdx + 1}:", outToks[1]);

                double[] xs = new double[n];
                double[] ys = new double[n];
                for (int i = 0; i < n; i++)
                {
                    xs[i] = double.Parse(outToks[2 + 2 * i], CultureInfo.InvariantCulture);
                    ys[i] = double.Parse(outToks[3 + 2 * i], CultureInfo.InvariantCulture);
                    Assert.IsTrue(double.IsFinite(xs[i]) && double.IsFinite(ys[i]));
                    Assert.IsTrue(xs[i] >= 0.0 && xs[i] <= w, $"Case #{caseIdx + 1}: x[{i}] out of [0, {w}]");
                    Assert.IsTrue(ys[i] >= 0.0 && ys[i] <= l, $"Case #{caseIdx + 1}: y[{i}] out of [0, {l}]");
                }

                for (int i = 0; i < n; i++)
                {
                    for (int j = i + 1; j < n; j++)
                    {
                        double dx = xs[i] - xs[j];
                        double dy = ys[i] - ys[j];
                        double minDist = radii[i] + radii[j];
                        Assert.IsTrue(dx * dx + dy * dy >= minDist * minDist - 1e-5,
                            $"Case #{caseIdx + 1}: students {i} and {j} overlap.");
                    }
                }
            }
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, ValidateQ2);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver, ValidateQ2);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver, ValidateQ2);
        }

        #endregion

        #region Question 3 - Mountain View

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out int[] nextOneBased);
            return () => Q3Solver(n, nextOneBased);
        }

        private static string Q3Solver(int n, int[] nextOneBased)
        {
            int[] nxt = new int[n - 1];
            for (int i = 0; i < n - 1; i++)
                nxt[i] = nextOneBased[i] - 1;

            int[] h = new int[n];
            Array.Fill(h, -1);
            int[] slope = new int[n];

            int p = 0;
            while (true)
            {
                h[p] = 1_000_000_000;
                slope[p] = 0;
                if (p == n - 1)
                    break;
                p = nxt[p];
            }

            List<int> chain = [];
            for (int a = 1; a < n - 1; a++)
            {
                if (h[a] != -1)
                    continue;
                int b = nxt[a - 1];
                int t = slope[a - 1];
                chain.Clear();
                int cur = a;
                while (cur < b)
                {
                    if (h[cur] != -1)
                        return "Impossible";
                    chain.Add(cur);
                    cur = nxt[cur];
                }
                if (cur != b)
                    return "Impossible";

                int newSlope = t + 1;
                foreach (int u in chain)
                {
                    h[u] = h[b] - newSlope * (b - u);
                    slope[u] = newSlope;
                }
            }

            return string.Join(" ", h);
        }

        private static bool TryParseStrictHeight(string token, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(token))
                return false;
            if ((token.Length > 1 && token[0] == '0') || token.StartsWith("-0", StringComparison.Ordinal))
                return false;
            if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return false;
            return value >= 0 && value <= 1_000_000_000;
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
                int n = int.Parse(inLines[2 * caseIdx + 1].Trim(), CultureInfo.InvariantCulture);
                string[] xToks = inLines[2 * caseIdx + 2].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int[] nxt = Array.ConvertAll(xToks, s => int.Parse(s, CultureInfo.InvariantCulture) - 1);

                bool expImpossible = expLines[caseIdx].Contains("Impossible", StringComparison.OrdinalIgnoreCase);
                string[] resToks = result[caseIdx].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.IsTrue(resToks.Length >= 3);
                Assert.AreEqual("Case", resToks[0]);
                Assert.AreEqual($"#{caseIdx + 1}:", resToks[1]);

                if (expImpossible)
                {
                    Assert.AreEqual(3, resToks.Length);
                    Assert.AreEqual("impossible", resToks[2].ToLowerInvariant());
                    continue;
                }

                Assert.AreEqual(2 + n, resToks.Length, $"Case #{caseIdx + 1}: wrong peak count.");
                int[] h = new int[n];
                for (int i = 0; i < n; i++)
                    Assert.IsTrue(TryParseStrictHeight(resToks[2 + i], out h[i]), $"Invalid height '{resToks[2 + i]}'");

                int[] prevPeaks = new int[n];
                Array.Fill(prevPeaks, -1);
                for (int peak = n - 2; peak >= 0; peak--)
                {
                    int v = nxt[peak];
                    long visDy = (long)h[v] - h[peak];
                    int visDx = v - peak;

                    if (v != n - 1)
                    {
                        int right = nxt[v];
                        long rDy = (long)h[right] - h[peak];
                        int rDx = right - peak;
                        Assert.IsTrue((Int128)rDy * visDx <= (Int128)visDy * rDx,
                            $"Case #{caseIdx + 1}: peak {right + 1} blocks {v + 1} from {peak + 1}.");
                    }

                    if (prevPeaks[v] >= 0)
                    {
                        int between = prevPeaks[v];
                        long bDy = (long)h[between] - h[peak];
                        int bDx = between - peak;
                        Assert.IsTrue((Int128)bDy * visDx < (Int128)visDy * bDx,
                            $"Case #{caseIdx + 1}: intermediate peak {between + 1} ties or exceeds {v + 1} from {peak + 1}.");
                    }

                    prevPeaks[v] = peak;
                }
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

        #region Question 4 - Descending in the Dark

        private readonly record struct HorizontalSegment(int Row, int C0, int Len, ulong GoodExitMask, ulong BadExitMask);

        private static Func<string[]> Q4Solver(GCJTestCase test)
        {
            test.Get(out int r, out int _);
            test.GetLines(r, out string[] grid);
            return () => Q4Solver(grid);
        }

        private static string[] Q4Solver(string[] grid)
        {
            int rows = grid.Length;
            int cols = grid[0].Length;
            (int R, int C)[] caves = new (int, int)[10];
            int caveCount = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    char ch = grid[r][c];
                    if (ch >= '0' && ch <= '9')
                    {
                        int id = ch - '0';
                        caves[id] = (r, c);
                        if (id + 1 > caveCount)
                            caveCount = id + 1;
                    }
                }
            }

            string[] output = new string[caveCount];
            bool[,] inS = new bool[rows, cols];
            Queue<(int R, int C)> bfs = new();

            for (int cid = 0; cid < caveCount; cid++)
            {
                var (cr, cc) = caves[cid];
                for (int r = 0; r <= Math.Min(rows - 1, cr + 1); r++)
                    for (int c = 0; c < cols; c++)
                        inS[r, c] = false;

                bfs.Clear();
                inS[cr, cc] = true;
                bfs.Enqueue((cr, cc));
                int reachable = 1;

                while (bfs.Count > 0)
                {
                    var (r, c) = bfs.Dequeue();
                    // Reverse moves of {DOWN, RIGHT, LEFT} are {UP, LEFT, RIGHT}
                    if (r > 0 && grid[r - 1][c] != '#' && !inS[r - 1, c])
                    {
                        inS[r - 1, c] = true;
                        bfs.Enqueue((r - 1, c));
                        reachable++;
                    }
                    if (c > 0 && grid[r][c - 1] != '#' && !inS[r, c - 1])
                    {
                        inS[r, c - 1] = true;
                        bfs.Enqueue((r, c - 1));
                        reachable++;
                    }
                    if (c + 1 < cols && grid[r][c + 1] != '#' && !inS[r, c + 1])
                    {
                        inS[r, c + 1] = true;
                        bfs.Enqueue((r, c + 1));
                        reachable++;
                    }
                }

                List<HorizontalSegment> segments = [];
                for (int r = 0; r <= cr; r++)
                {
                    int c = 0;
                    while (c < cols)
                    {
                        if (!inS[r, c])
                        {
                            c++;
                            continue;
                        }
                        int start = c;
                        while (c < cols && inS[r, c])
                            c++;
                        int len = c - start;
                        ulong good = 0, bad = 0;
                        if (r + 1 < rows)
                        {
                            for (int k = 0; k < len; k++)
                            {
                                int col = start + k;
                                if (inS[r + 1, col])
                                    good |= 1UL << k;
                                else if (grid[r + 1][col] != '#')
                                    bad |= 1UL << k;
                            }
                        }
                        segments.Add(new HorizontalSegment(r, start, len, good, bad));
                    }
                }

                ulong[] badByLen = new ulong[cols + 1];
                ulong[] possible = new ulong[cols + 1];
                while (segments.Count > 1)
                {
                    int maxLen = 0;
                    Array.Clear(badByLen);
                    foreach (var seg in segments)
                    {
                        badByLen[seg.Len] |= seg.BadExitMask;
                        if (seg.Len > maxLen)
                            maxLen = seg.Len;
                    }

                    possible[1] = 1UL & ~badByLen[1];
                    for (int len = 1; len < maxLen; len++)
                        possible[len + 1] = (possible[len] | (possible[len] << 1)) & ~badByLen[len + 1];

                    for (int len = maxLen; len >= 2; len--)
                        possible[len - 1] &= possible[len] | (possible[len] >> 1);

                    bool removedAny = false;
                    for (int i = segments.Count - 1; i >= 0; i--)
                    {
                        if ((segments[i].GoodExitMask & possible[segments[i].Len]) != 0)
                        {
                            segments.RemoveAt(i);
                            removedAny = true;
                        }
                    }

                    if (!removedAny)
                        break;
                }

                string status = segments.Count == 1 ? "Lucky" : "Unlucky";
                output[cid] = $"{cid}: {reachable} {status}";
            }

            return output;
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
    }
}
