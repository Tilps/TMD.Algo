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
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2016
{
    [TestFixture]
    public class FinalsTest : BaseGCJTest
    {
        #region Question 1 - Integeregex

        private Func<long> Q1Solver(GCJTestCase test)
        {
            long a, b;
            test.Get(out a, out b);
            string r;
            test.Get(out r);
            return () => Q1Solver(a, b, r);
        }

        private class Nfa
        {
            public int Start;
            public int End;
        }

        private class NfaBuilder
        {
            public int StateCount;
            public readonly List<int>[] Eps = new List<int>[70];
            public readonly List<int>[][] Digits = new List<int>[70][];

            public NfaBuilder()
            {
                for (int i = 0; i < 70; i++)
                {
                    Eps[i] = new List<int>();
                    Digits[i] = new List<int>[10];
                    for (int d = 0; d < 10; d++)
                        Digits[i][d] = new List<int>();
                }
            }

            public int NewState() => StateCount++;

            public Nfa MakeDigit(int d)
            {
                int s = NewState();
                int e = NewState();
                Digits[s][d].Add(e);
                return new Nfa { Start = s, End = e };
            }

            public Nfa MakeConcat(Nfa a, Nfa b)
            {
                Eps[a.End].Add(b.Start);
                return new Nfa { Start = a.Start, End = b.End };
            }

            public Nfa MakeDisjunction(List<Nfa> branches)
            {
                if (branches.Count == 1) return branches[0];
                int s = NewState();
                int e = NewState();
                foreach (var b in branches)
                {
                    Eps[s].Add(b.Start);
                    Eps[b.End].Add(e);
                }
                return new Nfa { Start = s, End = e };
            }

            public Nfa MakeRepetition(Nfa a)
            {
                int s = NewState();
                int e = NewState();
                Eps[s].Add(a.Start);
                Eps[s].Add(e);
                Eps[a.End].Add(a.Start);
                Eps[a.End].Add(e);
                return new Nfa { Start = s, End = e };
            }
        }

        private static Nfa ParseRegex(string s, NfaBuilder builder)
        {
            int pos = 0;
            return ParseDisjunction(s, ref pos, builder);
        }

        private static Nfa ParseDisjunction(string s, ref int pos, NfaBuilder builder)
        {
            List<Nfa> branches = new List<Nfa>();
            while (true)
            {
                branches.Add(ParseConcat(s, ref pos, builder));
                if (pos < s.Length && s[pos] == '|')
                    pos++;
                else
                    break;
            }
            return builder.MakeDisjunction(branches);
        }

        private static Nfa ParseConcat(string s, ref int pos, NfaBuilder builder)
        {
            List<Nfa> terms = new List<Nfa>();
            while (pos < s.Length && s[pos] != ')' && s[pos] != '|')
            {
                terms.Add(ParseTerm(s, ref pos, builder));
            }
            if (terms.Count == 0) throw new InvalidOperationException("Empty term sequence");
            Nfa result = terms[0];
            for (int i = 1; i < terms.Count; i++)
                result = builder.MakeConcat(result, terms[i]);
            return result;
        }

        private static Nfa ParseTerm(string s, ref int pos, NfaBuilder builder)
        {
            if (s[pos] >= '0' && s[pos] <= '9')
            {
                int digit = s[pos++] - '0';
                return builder.MakeDigit(digit);
            }
            if (s[pos] == '(')
            {
                pos++;
                Nfa inner = ParseDisjunction(s, ref pos, builder);
                pos++; // skip ')'
                if (pos < s.Length && s[pos] == '*')
                {
                    pos++;
                    return builder.MakeRepetition(inner);
                }
                return inner;
            }
            throw new InvalidOperationException($"Unexpected character: {s[pos]}");
        }

        private long Q1Solver(long a, long b, string r)
        {
            var builder = new NfaBuilder();
            Nfa nfa = ParseRegex(r, builder);
            int n = builder.StateCount;

            // Compute epsilon closures using Search.Bfs
            ulong[] epsClosure = new ulong[n];
            for (int i = 0; i < n; i++)
                Search.Bfs(i, u => epsClosure[i] |= 1UL << u, u => builder.Eps[u]);

            ulong Step(ulong mask, int digit)
            {
                ulong next = 0;
                ulong rem = mask;
                while (rem != 0)
                {
                    int i = BitTricks.LeastSignificantSetBitPos(rem);
                    rem = BitTricks.RemoveLeastSignificantSetBit(rem);
                    foreach (int dest in builder.Digits[i][digit])
                        next |= epsClosure[dest];
                }
                return next;
            }

            ulong initMask = epsClosure[nfa.Start];
            ulong endBit = 1UL << nfa.End;

            // Digit DP with memoization using TMD.Algo Memotize
            Func<int, ulong, long> countWays = null!;
            countWays = Memotize.CreateMemotizedRecursive<int, ulong, long>((recurse, remaining, mask) =>
            {
                if (remaining == 0)
                    return (mask & endBit) != 0 ? 1 : 0;

                long ways = 0;
                for (int d = 0; d < 10; d++)
                {
                    ulong next = Step(mask, d);
                    if (next != 0)
                        ways += recurse(remaining - 1, next);
                }
                return ways;
            });

            long CountUpTo(long val)
            {
                if (val <= 0) return 0;
                int[] digits = val.ToDigits().Reverse().ToArray();
                int len = digits.Length;
                long count = 0;

                // Numbers with fewer digits
                for (int l = 1; l < len; l++)
                {
                    for (int first = 1; first <= 9; first++)
                    {
                        ulong mask = Step(initMask, first);
                        if (mask != 0)
                            count += countWays(l - 1, mask);
                    }
                }

                // Numbers with same number of digits
                ulong current = initMask;
                for (int i = 0; i < len; i++)
                {
                    int limit = digits[i];
                    int first = (i == 0) ? 1 : 0;
                    for (int d = first; d < limit; d++)
                    {
                        ulong next = Step(current, d);
                        if (next != 0)
                            count += countWays(len - 1 - i, next);
                    }
                    current = Step(current, limit);
                    if (current == 0) break;
                }
                if (current != 0 && (current & endBit) != 0)
                    count++;

                return count;
            }

            return CountUpTo(b) - CountUpTo(a - 1);
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

        #region Question 2 - Family Hotel

        private const int MOD_Q2 = 1_000_000_007;
        private static readonly Modulo modQ2 = new Modulo(MOD_Q2);
        private static int[] fPrecomputed;
        private static readonly object fLock = new object();

        private static int[] GetF()
        {
            if (fPrecomputed != null) return fPrecomputed;
            lock (fLock)
            {
                if (fPrecomputed != null) return fPrecomputed;
                int maxN = 10_000_000;
                int[] f = new int[maxN + 1];
                f[1] = 1;

                int[] inv = new int[maxN + 1];
                inv[1] = 1;
                for (int i = 2; i <= maxN; i++)
                    inv[i] = (int)modQ2.Multiply(MOD_Q2 - MOD_Q2 / i, inv[MOD_Q2 % i]);

                long invFact = 1;
                for (int i = 1; i < maxN; i++)
                {
                    invFact = modQ2.Multiply(invFact, inv[i]);
                    long term = (i % 2 == 1) ? (MOD_Q2 - invFact) : invFact;
                    f[i + 1] = (int)modQ2.Add(f[i], term);
                }

                fPrecomputed = f;
                return fPrecomputed;
            }
        }

        private Func<int> Q2Solver(GCJTestCase test)
        {
            int n, k;
            test.Get(out n, out k);
            return () => Q2Solver(n, k);
        }

        private int Q2Solver(int n, int k)
        {
            int[] f = GetF();
            long fK = f[k];
            long fOther = f[n - k + 1];
            long prod = modQ2.Multiply(fK, fOther);
            long ans = modQ2.Subtract(1, prod);
            return (int)ans;
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

        #region Question 3 - Gallery of Pillars

        private static int[] mobiusPrecomputed;
        private static readonly object mobiusLock = new object();

        private static int[] GetMobius()
        {
            if (mobiusPrecomputed != null) return mobiusPrecomputed;
            lock (mobiusLock)
            {
                if (mobiusPrecomputed != null) return mobiusPrecomputed;
                mobiusPrecomputed = MathFormulas.MobiusSieve(1_000_000);
                return mobiusPrecomputed;
            }
        }

        private Func<long> Q3Solver(GCJTestCase test)
        {
            long n, r;
            test.Get(out n, out r);
            return () => Q3Solver(n, r);
        }

        private long Q3Solver(long n, long r)
        {
            long limit = (1_000_000_000_000L - 1) / (r * r);
            long maxCoord = Math.Min(n - 1, (long)Math.Sqrt(limit));
            if (maxCoord < 1) return 0;

            int[] mu = GetMobius();
            long countPairs = 0;
            int maxK = (int)maxCoord;

            for (int k = 1; k <= maxK; k++)
            {
                int muK = mu[k];
                if (muK == 0) continue;

                long lk = limit / ((long)k * k);
                long mk = maxCoord / k;
                if (lk < 5 || mk < 2) continue;

                long maxU = (long)Math.Sqrt((lk - 1) / 2);
                if (maxU >= mk) maxU = mk - 1;

                long subSum = 0;
                for (long u = 1; u <= maxU; u++)
                {
                    long maxV = (long)Math.Sqrt(lk - u * u);
                    if (maxV > mk) maxV = mk;
                    if (maxV > u)
                        subSum += (maxV - u);
                }
                countPairs += muK * subSum;
            }

            return 2 + 1 + 2 * countPairs;
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

        #region Question 4 - Map Reduce

        private Func<string> Q4Solver(GCJTestCase test)
        {
            int r, c, d;
            test.Get(out r, out c, out d);
            string[] rawGrid;
            test.GetLines(r, out rawGrid);
            return () => Q4Solver(r, c, d, rawGrid);
        }

        private static int BfsDistance(char[,] grid, int r, int c, (int r, int c) s, (int r, int c) f)
        {
            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };

            IEnumerable<(int r, int c)> GetNeighbours((int r, int c) cur)
            {
                for (int i = 0; i < 4; i++)
                {
                    int nr = cur.r + dr[i];
                    int nc = cur.c + dc[i];
                    if (nr >= 0 && nr < r && nc >= 0 && nc < c && grid[nr, nc] != '#')
                        yield return (nr, nc);
                }
            }

            return Search.Bfs(s, f, GetNeighbours, null, null);
        }

        private static bool IsWallRemovable(int i, int j, int r, int c, char[,] grid)
        {
            if (grid[i, j] != '#') return false;
            if (i <= 0 || i >= r - 1 || j <= 0 || j >= c - 1) return false;

            bool n = grid[i - 1, j] == '#';
            bool s = grid[i + 1, j] == '#';
            bool w = grid[i, j - 1] == '#';
            bool e = grid[i, j + 1] == '#';

            int wallNeighbors = (n ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0) + (e ? 1 : 0);
            if (wallNeighbors == 0) return true;
            if (wallNeighbors == 1) return true;
            if (wallNeighbors == 2)
            {
                if (n && e && grid[i - 1, j + 1] == '#') return true;
                if (n && w && grid[i - 1, j - 1] == '#') return true;
                if (s && e && grid[i + 1, j + 1] == '#') return true;
                if (s && w && grid[i + 1, j - 1] == '#') return true;
            }
            return false;
        }

        private string Q4Solver(int r, int c, int d, string[] rawGrid)
        {
            char[,] grid = new char[r, c];
            (int r, int c) s = (-1, -1);
            (int r, int c) f = (-1, -1);

            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    grid[i, j] = rawGrid[i][j];
                    if (grid[i, j] == 'S') s = (i, j);
                    if (grid[i, j] == 'F') f = (i, j);
                }
            }

            int initDist = BfsDistance(grid, r, c, s, f);
            int manhattan = Math.Abs(s.r - f.r) + Math.Abs(s.c - f.c);

            if (d > initDist || d < manhattan || ((d - initDist) & 1) != 0)
                return "IMPOSSIBLE";

            if (d == initDist)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("POSSIBLE");
                for (int i = 0; i < r; i++)
                {
                    for (int j = 0; j < c; j++)
                        sb.Append(grid[i, j]);
                    if (i + 1 < r) sb.AppendLine();
                }
                return sb.ToString();
            }

            // Find wall removal sequence via Search.Worklist
            List<(int r, int c)> removalOrder = new List<(int, int)>();

            IEnumerable<(int r, int c)> GetInitialRemovableWalls()
            {
                for (int i = 1; i < r - 1; i++)
                    for (int j = 1; j < c - 1; j++)
                        if (IsWallRemovable(i, j, r, c, grid))
                            yield return (i, j);
            }

            IEnumerable<(int r, int c)> GetRemovableNeighbours((int r, int c) cur)
            {
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        int nr = cur.r + dr;
                        int nc = cur.c + dc;
                        if (nr > 0 && nr < r - 1 && nc > 0 && nc < c - 1 && grid[nr, nc] == '#' && IsWallRemovable(nr, nc, r, c, grid))
                            yield return (nr, nc);
                    }
                }
            }

            Search.Worklist(
                GetInitialRemovableWalls(),
                cur =>
                {
                    if (!IsWallRemovable(cur.r, cur.c, r, c, grid))
                        return false;
                    grid[cur.r, cur.c] = '.';
                    removalOrder.Add(cur);
                    return true;
                },
                GetRemovableNeighbours);

            // Restore all removed walls back to '#'
            foreach (var (wr, wc) in removalOrder)
                grid[wr, wc] = '#';

            // Binary search for the number of walls to remove
            int applied = 0;
            SpecialtySelect.BinarySearch(0, removalOrder.Count, midLong =>
            {
                int mid = (int)midLong;
                while (applied < mid)
                {
                    var (wr, wc) = removalOrder[applied];
                    grid[wr, wc] = '.';
                    applied++;
                }
                while (applied > mid)
                {
                    applied--;
                    var (wr, wc) = removalOrder[applied];
                    grid[wr, wc] = '#';
                }

                int dist = BfsDistance(grid, r, c, s, f);
                return dist.CompareTo(d);
            });

            StringBuilder resSb = new StringBuilder();
            resSb.AppendLine("POSSIBLE");
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                    resSb.Append(grid[i, j]);
                if (i + 1 < r) resSb.AppendLine();
            }
            return resSb.ToString();
        }

        private void Q4CustomValidator(string input, string expectedOutput, List<string> result)
        {
            string[] inLines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int inT = int.Parse(inLines[0]);
            int inLineIdx = 1;

            List<string> expCases = new List<string>();
            string[] expLines = expectedOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder curCase = null;
            foreach (var line in expLines)
            {
                if (line.StartsWith("Case #"))
                {
                    if (curCase != null) expCases.Add(curCase.ToString());
                    curCase = new StringBuilder();
                }
                curCase?.AppendLine(line);
            }
            if (curCase != null) expCases.Add(curCase.ToString());

            Assert.AreEqual(inT, expCases.Count, "Expected cases count mismatch");
            Assert.AreEqual(inT, result.Count, "Result cases count mismatch");

            for (int t = 0; t < inT; t++)
            {
                string[] parts = inLines[inLineIdx].Split(' ');
                int r = int.Parse(parts[0]);
                int c = int.Parse(parts[1]);
                int d = int.Parse(parts[2]);
                inLineIdx++;

                string[] origGrid = new string[r];
                for (int i = 0; i < r; i++)
                    origGrid[i] = inLines[inLineIdx++];

                string expCaseStr = expCases[t];
                string resCaseStr = result[t];

                bool expPossible = expCaseStr.Contains("POSSIBLE") && !expCaseStr.Contains("IMPOSSIBLE");
                bool resPossible = resCaseStr.Contains("POSSIBLE") && !resCaseStr.Contains("IMPOSSIBLE");

                Assert.AreEqual(expPossible, resPossible, $"Case #{t + 1} feasibility mismatch");

                if (resPossible)
                {
                    string[] resLines = resCaseStr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    Assert.AreEqual(r + 1, resLines.Length, $"Case #{t + 1} row count mismatch");

                    char[,] grid = new char[r, c];
                    (int r, int c) s = (-1, -1);
                    (int r, int c) f = (-1, -1);
                    int emptyCount = 0;

                    for (int i = 0; i < r; i++)
                    {
                        string row = resLines[i + 1];
                        Assert.AreEqual(c, row.Length, $"Case #{t + 1} col count mismatch in row {i}");
                        for (int j = 0; j < c; j++)
                        {
                            char ch = row[j];
                            grid[i, j] = ch;
                            char origCh = origGrid[i][j];

                            if (ch == 'S') s = (i, j);
                            else if (ch == 'F') f = (i, j);

                            if (ch != origCh)
                            {
                                Assert.IsTrue(origCh == '#' && ch == '.',
                                    $"Case #{t + 1} invalid grid modification at ({i},{j}): {origCh} -> {ch}");
                            }

                            if (ch != '#') emptyCount++;
                        }
                    }

                    for (int i = 0; i < r; i++)
                    {
                        Assert.AreEqual('#', grid[i, 0], $"Case #{t + 1} left border not wall at row {i}");
                        Assert.AreEqual('#', grid[i, c - 1], $"Case #{t + 1} right border not wall at row {i}");
                    }
                    for (int j = 0; j < c; j++)
                    {
                        Assert.AreEqual('#', grid[0, j], $"Case #{t + 1} top border not wall at col {j}");
                        Assert.AreEqual('#', grid[r - 1, j], $"Case #{t + 1} bottom border not wall at col {j}");
                    }

                    for (int i = 0; i < r - 1; i++)
                    {
                        for (int j = 0; j < c - 1; j++)
                        {
                            bool tl = grid[i, j] == '#';
                            bool tr = grid[i, j + 1] == '#';
                            bool bl = grid[i + 1, j] == '#';
                            bool br = grid[i + 1, j + 1] == '#';

                            Assert.IsFalse(tl && br && !tr && !bl, $"Case #{t + 1} corner touch at ({i},{j})");
                            Assert.IsFalse(tr && bl && !tl && !br, $"Case #{t + 1} corner touch at ({i},{j})");
                        }
                    }

                    DisjointTracker<(int, int)> tracker = new DisjointTracker<(int, int)>();
                    for (int i = 0; i < r; i++)
                    {
                        for (int j = 0; j < c; j++)
                        {
                            if (grid[i, j] != '#')
                                tracker.Add((i, j));
                        }
                    }
                    for (int i = 0; i < r; i++)
                    {
                        for (int j = 0; j < c; j++)
                        {
                            if (grid[i, j] != '#')
                            {
                                if (i + 1 < r && grid[i + 1, j] != '#')
                                    tracker.Union((i, j), (i + 1, j));
                                if (j + 1 < c && grid[i, j + 1] != '#')
                                    tracker.Union((i, j), (i, j + 1));
                            }
                        }
                    }
                    for (int i = 0; i < r; i++)
                    {
                        for (int j = 0; j < c; j++)
                        {
                            if (grid[i, j] != '#')
                            {
                                Assert.AreEqual(tracker.GetRepresentative(s), tracker.GetRepresentative((i, j)),
                                    $"Case #{t + 1} disconnected empty squares at ({i},{j})");
                            }
                        }
                    }

                    int dist = BfsDistance(grid, r, c, s, f);
                    Assert.AreEqual(d, dist, $"Case #{t + 1} shortest path length mismatch");
                }
            }
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver, Q4CustomValidator);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver, Q4CustomValidator);
        }

        [Test]
        public void Q4Large()
        {
            SlowTest(Q4Solver, Q4CustomValidator);
        }

        #endregion

        #region Question 5 - Radioactive Islands

        private Func<double> Q5Solver(GCJTestCase test)
        {
            int n;
            double a, b;
            test.Get(out n, out a, out b);
            double[] c;
            test.Get(out c);
            return () => Q5Solver(n, a, b, c);
        }

        private double Q5Solver(int n, double a, double b, double[] c)
        {
            // System of ODEs:
            // state[0] = y
            // state[1] = y'
            // state[2] = cost
            Func<double, double[], double[]> deriv = (x, state) =>
            {
                double y = state[0];
                double yp = state[1];

                double v = 1.0;
                double dVdx = 0.0;
                double dVdy = 0.0;

                for (int i = 0; i < n; i++)
                {
                    double dx = x;
                    double dy = y - c[i];
                    double r2 = dx * dx + dy * dy;
                    if (r2 < 1e-12) r2 = 1e-12;
                    double invR2 = 1.0 / r2;
                    double invR4 = invR2 * invR2;

                    v += invR2;
                    dVdx += -2.0 * dx * invR4;
                    dVdy += -2.0 * dy * invR4;
                }

                double ypp = ((1.0 + yp * yp) / v) * (dVdy - dVdx * yp);
                double costP = v * Math.Sqrt(1.0 + yp * yp);

                return new double[] { yp, ypp, costP };
            };

            const int STEPS = 1000;

            (double yEnd, double cost) Simulate(double slope)
            {
                double[] state = NumericalMethods.RungeKutta4(deriv, -10.0, new double[] { a, slope, 0.0 }, 10.0, STEPS);
                return (state[0], state[2]);
            }


            double SimulateMidY(double slope)
            {
                double[] state = NumericalMethods.RungeKutta4(deriv, -10.0, new double[] { a, slope, 0.0 }, 0.0, STEPS / 2);
                return state[0];
            }

            // Find critical slopes where trajectory hits islands at x = 0
            double[] sortedC = (double[])c.Clone();
            Array.Sort(sortedC);

            double[] sCrit = new double[n];
            for (int i = 0; i < n; i++)
            {
                double targetY = sortedC[i];
                sCrit[i] = SpecialtySelect.BinarySearch(-10.0, 10.0, mid =>
                {
                    double y = SimulateMidY(mid);
                    return targetY.CompareTo(y);
                }, 45, out _);
            }

            List<(double sLo, double sHi)> intervals = new List<(double, double)>();
            const double EPS = 1e-5;
            intervals.Add((-10.0, sCrit[0] - EPS));
            for (int i = 0; i < n - 1; i++)
                intervals.Add((sCrit[i] + EPS, sCrit[i + 1] - EPS));
            intervals.Add((sCrit[n - 1] + EPS, 10.0));

            double minCost = double.MaxValue;
            foreach (var (sLo, sHi) in intervals)
            {
                if (sLo >= sHi) continue;
                double yLo = Simulate(sLo).yEnd;
                double yHi = Simulate(sHi).yEnd;

                if ((yLo - b) * (yHi - b) <= 0)
                {
                    bool increasing = yHi > yLo;
                    double optSlope = SpecialtySelect.BinarySearch(sLo, sHi, mid =>
                    {
                        double curY = Simulate(mid).yEnd;
                        return increasing ? b.CompareTo(curY) : curY.CompareTo(b);
                    }, 40, out _);
                    var (_, cost) = Simulate(optSlope);
                    if (cost < minCost)
                        minCost = cost;
                }
            }

            return minCost;
        }

        [Test]
        public void Q5Sample()
        {
            Test(Q5Solver, 1e-3);
        }

        [Test]
        public void Q5Small()
        {
            Test(Q5Solver, 1e-3);
        }

        [Test]
        public void Q5Large()
        {
            SlowTest(Q5Solver, 1e-3);
        }

        #endregion
    }
}
