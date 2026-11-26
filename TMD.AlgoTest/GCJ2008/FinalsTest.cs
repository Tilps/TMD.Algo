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
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2008
{
    [TestFixture]
    public class FinalsTest : BaseGCJTest
    {
        #region Question 1 - Juice

        private Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines<int, int, int>(n, out (int a, int b, int c)[] guests);
            return () => Q1Solve(guests);
        }

        private int Q1Solve((int a, int b, int c)[] guests)
        {
            if (guests.Length == 0) return 0;

            var uniqueC = new HashSet<int>();
            foreach (var g in guests)
                uniqueC.Add(g.c);

            int maxSatisfied = 0;
            int[] diff = new int[10002];

            foreach (int cTarget in uniqueC)
            {
                int eligibleCount = 0;
                foreach (var g in guests)
                {
                    if (g.c <= cTarget && g.a + g.b + cTarget <= 10000)
                    {
                        diff[g.a]++;
                        diff[10000 - cTarget - g.b + 1]--;
                        eligibleCount++;
                    }
                }

                if (eligibleCount <= maxSatisfied)
                {
                    foreach (var g in guests)
                    {
                        if (g.c <= cTarget && g.a + g.b + cTarget <= 10000)
                        {
                            diff[g.a] = 0;
                            diff[10000 - cTarget - g.b + 1] = 0;
                        }
                    }
                    continue;
                }

                int cur = 0;
                int limit = 10000 - cTarget;
                for (int a = 0; a <= limit; a++)
                {
                    cur += diff[a];
                    if (cur > maxSatisfied)
                        maxSatisfied = cur;
                }

                foreach (var g in guests)
                {
                    if (g.c <= cTarget && g.a + g.b + cTarget <= 10000)
                    {
                        diff[g.a] = 0;
                        diff[10000 - cTarget - g.b + 1] = 0;
                    }
                }
            }

            return maxSatisfied;
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

        #region Question 2 - Ping Pong Balls

        private Func<long> Q2Solver(GCJTestCase test)
        {
            test.Get(out long w, out long h);
            test.Get(out long dx1, out long dy1);
            test.Get(out long dx2, out long dy2);
            test.Get(out long x0, out long y0);
            return () => Q2Solve(w, h, dx1, dy1, dx2, dy2, x0, y0);
        }

        private long Q2Solve(long w, long h, long dx1, long dy1, long dx2, long dy2, long x0, long y0)
        {
            long det = dx1 * dy2 - dx2 * dy1;
            if (det == 0)
                return Q2SolveCollinear(w, h, dx1, dy1, dx2, dy2, x0, y0);

            long total = 0;
            if (Q2GetRangeB(w, h, dx1, dy1, dx2, dy2, x0, y0, 0, out long lowB0, out long highB0))
            {
                if (lowB0 <= 0 && 0 <= highB0)
                {
                    long reachLow = 0;
                    long reachHigh = highB0;
                    total += (reachHigh - reachLow + 1);

                    long a = 1;
                    while (reachLow <= reachHigh)
                    {
                        if (!Q2GetRangeB(w, h, dx1, dy1, dx2, dy2, x0, y0, a, out long lowB, out long highB))
                            break;

                        long entryLow = Math.Max(reachLow, lowB);
                        if (entryLow > reachHigh || entryLow > highB)
                            break;

                        reachLow = entryLow;
                        reachHigh = highB;
                        total += (reachHigh - reachLow + 1);
                        a++;
                    }
                }
            }
            return total;
        }

        private bool Q2GetRangeB(long w, long h, long dx1, long dy1, long dx2, long dy2, long x0, long y0, long a,
            out long lowB, out long highB)
        {
            lowB = 0;
            highB = long.MaxValue;

            long curX = x0 + a * dx1;
            if (!Q2Bound(dx2, -curX, w - 1 - curX, ref lowB, ref highB)) return false;

            long curY = y0 + a * dy1;
            if (!Q2Bound(dy2, -curY, h - 1 - curY, ref lowB, ref highB)) return false;

            return lowB <= highB;
        }

        private bool Q2Bound(long step, long minVal, long maxVal, ref long low, ref long high)
        {
            if (step == 0)
                return 0 >= minVal && 0 <= maxVal;

            long l, r;
            if (step > 0)
            {
                l = MathFormulas.DivCeil(minVal, step);
                r = MathFormulas.DivFloor(maxVal, step);
            }
            else
            {
                l = MathFormulas.DivCeil(maxVal, step);
                r = MathFormulas.DivFloor(minVal, step);
            }
            low = Math.Max(low, l);
            high = Math.Min(high, r);
            return low <= high;
        }

        private long Q2SolveCollinear(long w, long h, long dx1, long dy1, long dx2, long dy2, long x0, long y0)
        {
            long gx = dx1 != 0 ? dx1 : dx2;
            long gy = dy1 != 0 ? dy1 : dy2;
            if (gx == 0 && gy == 0) return 1;

            long g = MathFormulas.Gcd(Math.Abs(gx), Math.Abs(gy));
            gx /= g;
            gy /= g;
            if (gx < 0 || (gx == 0 && gy < 0))
            {
                gx = -gx;
                gy = -gy;
            }

            long kMin = long.MinValue, kMax = long.MaxValue;
            if (gx != 0)
            {
                kMin = Math.Max(kMin, MathFormulas.DivCeil(-x0, gx));
                kMax = Math.Min(kMax, MathFormulas.DivFloor(w - 1 - x0, gx));
            }
            if (gy != 0)
            {
                if (gy > 0)
                {
                    kMin = Math.Max(kMin, MathFormulas.DivCeil(-y0, gy));
                    kMax = Math.Min(kMax, MathFormulas.DivFloor(h - 1 - y0, gy));
                }
                else
                {
                    kMin = Math.Max(kMin, MathFormulas.DivCeil(h - 1 - y0, gy));
                    kMax = Math.Min(kMax, MathFormulas.DivFloor(-y0, gy));
                }
            }

            long s1 = gx != 0 ? dx1 / gx : (gy != 0 ? dy1 / gy : 0);
            long s2 = gx != 0 ? dx2 / gx : (gy != 0 ? dy2 / gy : 0);

            int size = (int)(kMax - kMin + 1);
            if (size <= 0) return 0;

            bool[] visited = new bool[size];
            int startK = (int)(0 - kMin);
            if (startK < 0 || startK >= size) return 0;

            var queue = new Queue<int>();
            visited[startK] = true;
            queue.Enqueue(0);
            long count = 0;

            while (queue.Count > 0)
            {
                long cur = queue.Dequeue();
                count++;

                long next1 = cur + s1;
                if (next1 >= kMin && next1 <= kMax)
                {
                    int idx = (int)(next1 - kMin);
                    if (!visited[idx])
                    {
                        visited[idx] = true;
                        queue.Enqueue((int)next1);
                    }
                }

                long next2 = cur + s2;
                if (next2 >= kMin && next2 <= kMax)
                {
                    int idx = (int)(next2 - kMin);
                    if (!visited[idx])
                    {
                        visited[idx] = true;
                        queue.Enqueue((int)next2);
                    }
                }
            }
            return count;
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

        #region Question 3 - Mine Layer

        private Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c);
            test.GetMatrix(r, out int[][] clues);
            return () => Q3Solve(r, c, clues);
        }

        private int Q3Solve(int r, int c, int[][] clues)
        {
            int h = (r - 1) / 2;

            int SumRow(int rowIdx)
            {
                int sum = 0;
                int start = (c % 3 != 0) ? 0 : 1;
                for (int col = start; col < c; col += 3)
                    sum += clues[rowIdx][col];
                return sum;
            }

            int total = 0;
            int rStart = (r % 3 != 0) ? 0 : 1;
            for (int row = rStart; row < r; row += 3)
                total += SumRow(row);

            if (h % 3 == 1)
            {
                int s = 0;
                for (int row = h - 1; row >= 0; row -= 3) s += SumRow(row);
                for (int row = h + 1; row < r; row += 3) s += SumRow(row);
                return s - total;
            }
            else
            {
                int s = 0;
                for (int row = h - 2; row >= 0; row -= 3) s += SumRow(row);
                for (int row = h + 2; row < r; row += 3) s += SumRow(row);
                return total - s;
            }
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

        #region Question 4 - Bridge Builders

        private Func<int> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            test.GetLines(n, out string[] grid);
            return () => Q4Solve(n, m, grid);
        }

        private int Q4Solve(int n, int m, string[] grid)
        {
            var forests = new List<(int r, int c)>();
            var islands = new List<(int r, int c)>();

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < m; c++)
                {
                    if (grid[r][c] == 'T')
                    {
                        forests.Add((r, c));
                        islands.Add((r, c));
                    }
                    else if (grid[r][c] == '#')
                    {
                        islands.Add((r, c));
                    }
                }
            }

            int k = forests.Count;
            int[,] dist = new int[k, n * m];

            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };

            for (int f = 0; f < k; f++)
            {
                for (int i = 0; i < n * m; i++) dist[f, i] = -1;
                var (fr, fc) = forests[f];
                dist[f, fr * m + fc] = 0;

                Search.Bfs((fr, fc), (cur, parent, depth) =>
                {
                    dist[f, cur.fr * m + cur.fc] = depth;
                }, cur =>
                {
                    var neighbors = new List<(int fr, int fc)>(4);
                    for (int dir = 0; dir < 4; dir++)
                    {
                        int nr = cur.fr + dr[dir];
                        int nc = cur.fc + dc[dir];
                        if (nr >= 0 && nr < n && nc >= 0 && nc < m && grid[nr][nc] != '.')
                            neighbors.Add((nr, nc));
                    }
                    return neighbors;
                });
            }

            var forestGraph = new Graph<int, int>();
            for (int f = 0; f < k; f++)
                forestGraph.AddVertex(f);

            for (int i = 0; i < k; i++)
            {
                for (int j = i + 1; j < k; j++)
                {
                    var (fr, fc) = forests[j];
                    int x = dist[i, fr * m + fc];
                    if (x >= 0)
                    {
                        int y = x / 2;
                        int cost = x * (x + 1) / 2 - y * (y + 1);
                        if (x % 2 == 0) cost += y;
                        forestGraph.AddUndirectedEdge(i, j, cost);
                    }
                }
            }

            var mstEdgeIds = forestGraph.FindMinimumSpanningTree();
            int total = mstEdgeIds.Sum(edgeId => forestGraph.GetEdge(edgeId));

            foreach (var (r, c) in islands)
            {
                int cell = r * m + c;
                int minDist = int.MaxValue;
                for (int f = 0; f < k; f++)
                {
                    if (dist[f, cell] >= 0 && dist[f, cell] < minDist)
                        minDist = dist[f, cell];
                }
                if (minDist < int.MaxValue)
                    total += minDist;
            }

            return total;
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

        #region Question 5 - The Year of Code Jam

        private Func<long> Q5Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            test.GetLines(n, out string[] grid);
            return () => Q5Solve(n, m, grid);
        }

        private long Q5Solve(int n, int m, string[] grid)
        {
            int totalSegments = 2 * n * m + n + m;
            const int inf = 1000000;

            var graph = new Graph<int, int>();
            for (int i = 0; i < n * m + 2; i++)
                graph.AddVertex(i);

            int source = n * m;
            int sink = n * m + 1;

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < m; c++)
                {
                    int u = r * m + c;
                    bool isEven = (r + c) % 2 == 0;

                    int boundaryCount = 0;
                    if (r == 0) boundaryCount++;
                    if (r == n - 1) boundaryCount++;
                    if (c == 0) boundaryCount++;
                    if (c == m - 1) boundaryCount++;

                    if (isEven)
                    {
                        if (boundaryCount > 0)
                            graph.AddDirectedEdge(source, u, boundaryCount);
                    }
                    else
                    {
                        if (boundaryCount > 0)
                            graph.AddDirectedEdge(u, sink, boundaryCount);
                    }

                    if (c + 1 < m)
                    {
                        int v = r * m + (c + 1);
                        graph.AddUndirectedEdge(u, v, 1);
                    }
                    if (r + 1 < n)
                    {
                        int v = (r + 1) * m + c;
                        graph.AddUndirectedEdge(u, v, 1);
                    }

                    char ch = grid[r][c];
                    if (ch == '#')
                    {
                        if (isEven)
                            graph.AddDirectedEdge(source, u, inf);
                        else
                            graph.AddDirectedEdge(u, sink, inf);
                    }
                    else if (ch == '.')
                    {
                        if (isEven)
                            graph.AddDirectedEdge(u, sink, inf);
                        else
                            graph.AddDirectedEdge(source, u, inf);
                    }
                }
            }

            long maxFlow = graph.MaximumFlowDinic(source, sink);
            return totalSegments - maxFlow;
        }

        [Test]
        public void Q5Sample()
        {
            Test(Q5Solver);
        }

        [Test]
        public void Q5Small()
        {
            Test(Q5Solver);
        }

        [Test]
        public void Q5Large()
        {
            Test(Q5Solver);
        }

        #endregion
    }
}
