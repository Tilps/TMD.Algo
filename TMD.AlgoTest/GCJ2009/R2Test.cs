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
    public class R2Test : BaseGCJTest
    {
        #region Question 1 - Crazy Rows

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out string[] rows);
            return () => Q1Solver(n, rows);
        }

        private static int Q1Solver(int n, string[] rows)
        {
            int[] lastOne = new int[n];
            for (int r = 0; r < n; r++)
            {
                lastOne[r] = rows[r].LastIndexOf('1');
            }

            int swaps = 0;
            for (int i = 0; i < n; i++)
            {
                int found = i;
                while (lastOne[found] > i)
                {
                    found++;
                }

                swaps += found - i;
                int val = lastOne[found];
                for (int k = found; k > i; k--)
                {
                    lastOne[k] = lastOne[k - 1];
                }
                lastOne[i] = val;
            }

            return swaps;
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

        #region Question 2 - A Digging Problem

        private readonly record struct DigState(int Row, int WalkL, int WalkR, bool DropLeft, bool DropRight);

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c, out int f);
            test.GetLines(r, out string[] grid);
            return () => Q2Solver(r, c, f, grid);
        }

        private static string Q2Solver(int rows, int cols, int maxFall, string[] grid)
        {
            bool HasSupportBelow(int r, int c) => r == rows - 1 || grid[r + 1][c] == '#';

            bool IsEmptyWithDug(int r, int c, int dugL, int dugR) =>
                grid[r][c] == '.' || (c >= dugL && c <= dugR);

            DigState MakeState(int r, int startCol, int dugL, int dugR)
            {
                int walkL = startCol;
                while (walkL > 0 && IsEmptyWithDug(r, walkL - 1, dugL, dugR) && HasSupportBelow(r, walkL - 1))
                {
                    walkL--;
                }

                int walkR = startCol;
                while (walkR + 1 < cols && IsEmptyWithDug(r, walkR + 1, dugL, dugR) && HasSupportBelow(r, walkR + 1))
                {
                    walkR++;
                }

                bool dropLeft = walkL > 0 && IsEmptyWithDug(r, walkL - 1, dugL, dugR);
                bool dropRight = walkR + 1 < cols && IsEmptyWithDug(r, walkR + 1, dugL, dugR);
                return new DigState(r, walkL, walkR, dropLeft, dropRight);
            }

            int DropToSupportedRow(int fromRow, int col)
            {
                int r = fromRow + 1;
                while (r + 1 < rows && grid[r + 1][col] == '.')
                {
                    r++;
                }
                return r;
            }

            IEnumerable<KeyValuePair<DigState, int>> Transitions(DigState state, int digs)
            {
                if (state.Row >= rows - 1)
                    yield break;

                if (state.DropLeft)
                {
                    int col = state.WalkL - 1;
                    int destR = DropToSupportedRow(state.Row, col);
                    if (destR - state.Row <= maxFall)
                    {
                        yield return new KeyValuePair<DigState, int>(MakeState(destR, col, -1, -1), digs);
                    }
                }

                if (state.DropRight)
                {
                    int col = state.WalkR + 1;
                    int destR = DropToSupportedRow(state.Row, col);
                    if (destR - state.Row <= maxFall)
                    {
                        yield return new KeyValuePair<DigState, int>(MakeState(destR, col, -1, -1), digs);
                    }
                }

                for (int a = state.WalkL; a <= state.WalkR; a++)
                {
                    for (int b = a; b <= state.WalkR; b++)
                    {
                        if (a == state.WalkL && b == state.WalkR)
                            continue;

                        int nextDigs = digs + (b - a + 1);

                        // Exit downward on right end (b) by standing at b + 1 for the final dig.
                        if (b < state.WalkR)
                        {
                            if (HasSupportBelow(state.Row + 1, b))
                            {
                                yield return new KeyValuePair<DigState, int>(
                                    MakeState(state.Row + 1, b, a, b), nextDigs);
                            }
                            else
                            {
                                int destR = DropToSupportedRow(state.Row, b);
                                if (destR - state.Row <= maxFall)
                                {
                                    yield return new KeyValuePair<DigState, int>(
                                        MakeState(destR, b, -1, -1), nextDigs);
                                }
                            }
                        }

                        // Exit downward on left end (a) by standing at a - 1 for the final dig.
                        if (a > state.WalkL && a != b || (a > state.WalkL && b == state.WalkR))
                        {
                            if (HasSupportBelow(state.Row + 1, a))
                            {
                                yield return new KeyValuePair<DigState, int>(
                                    MakeState(state.Row + 1, a, a, b), nextDigs);
                            }
                            else
                            {
                                int destR = DropToSupportedRow(state.Row, a);
                                if (destR - state.Row <= maxFall)
                                {
                                    yield return new KeyValuePair<DigState, int>(
                                        MakeState(destR, a, -1, -1), nextDigs);
                                }
                            }
                        }
                    }
                }
            }

            DigState initial = MakeState(0, 0, -1, -1);
            bool found = Search.TryPfs(initial, 0, s => s.Row == rows - 1, Transitions, out int minDigs);
            return found ? $"Yes {minDigs}" : "No";
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

        #region Question 3 - Stock Charts

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n, out int k);
            test.GetMatrix(n, out int[][] prices);
            return () => Q3Solver(n, k, prices);
        }

        private static int Q3Solver(int n, int k, int[][] prices)
        {
            var result = BipartiteMatching.FindMaximumMatching(n, n, u =>
            {
                List<int> edges = [];
                for (int v = 0; v < n; v++)
                {
                    if (u == v)
                        continue;

                    bool strictlyAbove = true;
                    for (int t = 0; t < k; t++)
                    {
                        if (prices[u][t] <= prices[v][t])
                        {
                            strictlyAbove = false;
                            break;
                        }
                    }

                    if (strictlyAbove)
                        edges.Add(v);
                }
                return edges;
            });

            return n - result.Count;
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

        #region Question 4 - Watering Plants

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out (int X, int Y, int R)[] plants);
            return () => Q4Solver(plants);
        }

        private static string Q4Solver((int X, int Y, int R)[] plants)
        {
            int n = plants.Length;
            ulong allMask = (1UL << n) - 1;
            double minPossible = 0.0;
            foreach (var p in plants)
            {
                if (p.R > minPossible)
                    minPossible = p.R;
            }

            bool CanCover(double radius)
            {
                double[] maxCenterDist = new double[n];
                double[] maxCenterDistSq = new double[n];
                for (int i = 0; i < n; i++)
                {
                    double d = radius - plants[i].R;
                    if (d < -1e-12)
                        return false;
                    d = Math.Max(0.0, d);
                    maxCenterDist[i] = d;
                    maxCenterDistSq[i] = (d + 1e-11) * (d + 1e-11);
                }

                HashSet<ulong> uniqueMasks = [];

                void AddCandidate(double cx, double cy)
                {
                    ulong mask = 0;
                    for (int k = 0; k < n; k++)
                    {
                        double dx = cx - plants[k].X;
                        double dy = cy - plants[k].Y;
                        if (dx * dx + dy * dy <= maxCenterDistSq[k])
                            mask |= 1UL << k;
                    }
                    if (mask != 0)
                        uniqueMasks.Add(mask);
                }

                for (int i = 0; i < n; i++)
                {
                    AddCandidate(plants[i].X, plants[i].Y);
                    for (int j = i + 1; j < n; j++)
                    {
                        double dx = plants[j].X - plants[i].X;
                        double dy = plants[j].Y - plants[i].Y;
                        double distSq = dx * dx + dy * dy;
                        double r1 = maxCenterDist[i];
                        double r2 = maxCenterDist[j];
                        double sumR = r1 + r2;
                        double diffR = Math.Abs(r1 - r2);

                        if (distSq <= 0.0 || distSq > (sumR + 1e-12) * (sumR + 1e-12) ||
                            distSq < (diffR - 1e-12) * (diffR - 1e-12))
                        {
                            continue;
                        }

                        double dist = Math.Sqrt(distSq);
                        double a = (r1 * r1 - r2 * r2 + distSq) / (2.0 * dist);
                        double hSq = Math.Max(0.0, r1 * r1 - a * a);
                        double h = Math.Sqrt(hSq);

                        double mx = plants[i].X + a * dx / dist;
                        double my = plants[i].Y + a * dy / dist;
                        double rx = -dy * (h / dist);
                        double ry = dx * (h / dist);

                        AddCandidate(mx + rx, my + ry);
                        if (h > 1e-12)
                            AddCandidate(mx - rx, my - ry);
                    }
                }

                if (uniqueMasks.Contains(allMask))
                    return true;

                ulong[] masks = [.. uniqueMasks];
                for (int i = 0; i < masks.Length; i++)
                {
                    ulong mi = masks[i];
                    for (int j = i + 1; j < masks.Length; j++)
                    {
                        if ((mi | masks[j]) == allMask)
                            return true;
                    }
                }

                return false;
            }

            double bestRadius = CanCover(minPossible)
                ? minPossible
                : SpecialtySelect.FindTransition(minPossible, 1500.0, CanCover);
            return $"{bestRadius:F6}";
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver, 1e-5);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver, 1e-5);
        }

        [Test]
        public void Q4Large()
        {
            SlowTest(Q4Solver, 1e-5);
        }

        #endregion
    }
}
